using System.Security.Claims;
using HeartClinicHms.Api.Services;
using HeartClinicHms.Domain.Entities;
using HeartClinicHms.Domain.Enums;
using HeartClinicHms.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var useSqlite = builder.Configuration.GetValue<bool>("UseSqlite")
    || builder.Environment.IsEnvironment("Demo")
    || (connectionString != null && (connectionString.Contains(".db", StringComparison.OrdinalIgnoreCase) || (connectionString.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase) && !connectionString.Contains("Server="))));

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (useSqlite)
    {
        var sqliteConn = !string.IsNullOrWhiteSpace(connectionString) && connectionString.Contains("Data Source=")
            ? connectionString
            : "Data Source=HeartClinicDemo.db";
        options.UseSqlite(sqliteConn);
    }
    else
    {
        options.UseSqlServer(connectionString);
    }
    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});

builder.Services.AddScoped<MrNumberService>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var configuredOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>();
        if (builder.Environment.IsProduction() && configuredOrigins is { Length: > 0 } && !configuredOrigins.Contains("*"))
        {
            policy.WithOrigins(configuredOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
        else
        {
            policy.SetIsOriginAllowed(_ => true)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    });
});

var jwtKey = builder.Configuration["Jwt:Key"] ?? "AAQSOLS-PulseCore-HeartClinic-Dev-Key-Change-In-Production-2026";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "HeartClinicHms",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "HeartClinicHms.Web",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (useSqlite)
    {
        await db.Database.EnsureCreatedAsync();
    }
    else
    {
        if (app.Environment.IsDevelopment())
        {
            try
            {
                await db.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                app.Logger.LogWarning(ex, "Migration failed — recreating database.");
                await db.Database.EnsureDeletedAsync();
                await db.Database.MigrateAsync();
            }
        }
        else
        {
            await db.Database.MigrateAsync();
        }
    }
    await DatabaseSeeder.SeedAsync(db);
}

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", product = "PulseCore Heart Clinic HMS", database = useSqlite ? "SQLite (Demo)" : "SQL Server" }));

app.MapPost("/api/auth/login", async (LoginRequest req, AppDbContext db, IConfiguration config) =>
{
    var user = await db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserName == req.UserName && u.IsActive);
    if (user is null || !AuthService.VerifyPassword(user, req.Password))
        return Results.Unauthorized();

    user.LastLoginAtUtc = DateTime.UtcNow;
    await db.SaveChangesAsync();
    return Results.Ok(new LoginResponse(AuthService.CreateToken(user, config), user.DisplayName, user.Role.Code, user.Role.Name));
});

app.MapGet("/api/patients", async (string? search, bool allBranches, AppDbContext db, ClaimsPrincipal user) =>
{
    var branchId = Guid.Parse(user.FindFirstValue("branchId")!);
    var query = db.Patients.Include(p => p.PatientType).Include(p => p.CorporatePanel).Include(p => p.Branch).AsQueryable();

    if (!allBranches)
        query = query.Where(p => p.BranchId == branchId);

    if (!string.IsNullOrWhiteSpace(search))
    {
        var term = search.Trim();
        query = query.Where(p =>
            p.FirstName.Contains(term) ||
            p.MrNumber.Contains(term) ||
            p.CnicOrPassport.Contains(term));
    }

    var patients = await query.OrderByDescending(p => p.CreatedAtUtc).Take(200).Select(p => new PatientDto(
        p.Id, p.FirstName, p.GuardianName, p.MrNumber, p.CnicOrPassport, p.Gender.ToString(),
        p.PatientType.Name, p.Mobile, p.CreatedAtUtc, p.CorporatePanelId,
        p.CorporatePanel != null ? p.CorporatePanel.Name : null,
        p.Branch.Name, p.Branch.Region)).ToListAsync();
    return Results.Ok(patients);
}).RequireAuthorization();

app.MapPost("/api/patients", async (CreatePatientRequest req, AppDbContext db, MrNumberService mrService, ClaimsPrincipal user) =>
{
    if (await db.Patients.AnyAsync(p => p.CnicOrPassport == req.CnicOrPassport))
        return Results.Conflict(new { message = "A patient with this CNIC already exists." });

    var branchId = Guid.Parse(user.FindFirstValue("branchId")!);
    var type = await db.PatientTypes.FirstOrDefaultAsync(t => t.Code == req.PatientTypeCode)
        ?? await db.PatientTypes.FirstAsync();

    var patient = new Patient
    {
        BranchId = branchId,
        PatientTypeId = type.Id,
        MrNumber = await mrService.GenerateNextAsync(branchId),
        FirstName = req.FirstName,
        GuardianName = req.GuardianName,
        Gender = Enum.Parse<PatientGender>(req.Gender, true),
        CnicOrPassport = req.CnicOrPassport,
        Mobile = req.Mobile,
        Address = req.Address
    };
    db.Patients.Add(patient);
    await db.SaveChangesAsync();
    return Results.Created($"/api/patients/{patient.Id}", new { patient.Id, patient.MrNumber });
}).RequireAuthorization();

app.MapGet("/api/doctors", async (AppDbContext db) =>
{
    var doctors = await db.Doctors.Where(d => d.IsActive)
        .Select(d => new DoctorDto(d.Id, d.FullName, d.Specialty ?? "")).ToListAsync();
    return Results.Ok(doctors);
}).RequireAuthorization();

app.MapGet("/api/services", async (AppDbContext db) =>
{
    var groups = await db.ServiceGroups.Include(g => g.Services)
        .Where(g => g.IsActive)
        .Select(g => new ServiceGroupDto(g.Id, g.Name, g.Services.Where(s => s.IsActive)
            .Select(s => new ServiceDto(s.Id, s.Name, s.DefaultCharges, s.DefaultDeliveryDays)).ToList())).ToListAsync();
    return Results.Ok(groups);
}).RequireAuthorization();

app.MapPost("/api/check-ins", async (CreateCheckInRequest req, AppDbContext db, ClaimsPrincipal user) =>
{
    if (req.ServiceLines.Count == 0)
        return Results.BadRequest(new { message = "At least one service is required." });

    var createdBy = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
    var branchId = Guid.Parse(user.FindFirstValue("branchId")!);

    var patient = await db.Patients.Include(p => p.PatientType).FirstOrDefaultAsync(p => p.Id == req.PatientId);
    if (patient is null) return Results.NotFound();

    var isPanel = patient.PatientType.Code == "PANEL" && patient.CorporatePanelId.HasValue;
    PanelBillingMode? panelMode = null;
    if (isPanel && !string.IsNullOrWhiteSpace(req.PanelBillingMode))
        panelMode = Enum.Parse<PanelBillingMode>(req.PanelBillingMode, true);

    var resolvedLines = new List<(Guid ServiceId, string Name, decimal Charges, DateOnly? Delivery)>();
    foreach (var line in req.ServiceLines)
    {
        var catalog = await db.CatalogServices.FirstOrDefaultAsync(s => s.Id == line.ServiceId);
        var baseCharge = catalog?.DefaultCharges ?? line.Charges;
        var charge = await PanelBillingHelper.ResolveChargeAsync(db, line.ServiceId, baseCharge, patient.CorporatePanelId);
        resolvedLines.Add((line.ServiceId, line.ServiceName, charge, line.DeliveryDate));
    }

    var subTotal = resolvedLines.Sum(l => l.Charges);
    var (patientPayable, panelPayable, mode) = PanelBillingHelper.SplitTotal(subTotal, isPanel, panelMode);

    var checkIn = new CheckIn
    {
        BranchId = branchId,
        PatientId = req.PatientId,
        DoctorId = req.DoctorId,
        CheckedInByUserId = createdBy,
        CheckInType = Enum.Parse<CheckInType>(req.CheckInType, true),
        Destination = Enum.Parse<CheckInDestination>(req.Destination, true),
        PrescribedBy = Enum.Parse<PrescribedBy>(req.PrescribedBy, true),
        SmsAlert = Enum.Parse<SmsAlertOption>(req.SmsAlert, true),
        PackageName = req.PackageName,
        SubTotal = subTotal,
        Status = CheckInStatus.CheckedIn
    };

    foreach (var line in resolvedLines)
    {
        checkIn.ServiceLines.Add(new CheckInServiceLine
        {
            CatalogServiceId = line.ServiceId,
            ServiceName = line.Name,
            Charges = line.Charges,
            DeliveryDate = line.Delivery
        });
    }

    var challanNo = $"CH-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
    checkIn.Challan = new Challan
    {
        BranchId = branchId,
        ChallanNumber = challanNo,
        SubTotal = subTotal,
        PatientPayable = patientPayable,
        PanelPayable = panelPayable,
        PanelBillingMode = mode,
        CorporatePanelId = isPanel ? patient.CorporatePanelId : null,
        IssuedByUserId = createdBy
    };

    db.CheckIns.Add(checkIn);
    await db.SaveChangesAsync();

    foreach (var line in checkIn.ServiceLines)
    {
        var catalog = await db.CatalogServices.FirstAsync(s => s.Id == line.CatalogServiceId);
        if (catalog.IsDiagnostic)
        {
            db.LabWorkItems.Add(new LabWorkItem
            {
                CheckInServiceLineId = line.Id,
                Status = LabWorkStatus.PendingPayment
            });
        }
    }
    await db.SaveChangesAsync();

    return Results.Ok(new CheckInResultDto(checkIn.Id, challanNo, subTotal, patientPayable, panelPayable, mode?.ToString()));
}).RequireAuthorization();

app.MapGet("/api/check-ins/active", async (AppDbContext db) =>
{
    var items = await db.CheckIns
        .Include(c => c.Patient).ThenInclude(p => p.PatientType)
        .Include(c => c.Doctor)
        .Where(c => c.Status == CheckInStatus.CheckedIn || c.Status == CheckInStatus.InConsultation || c.Status == CheckInStatus.OnHold)
        .OrderByDescending(c => c.CheckedInAtUtc)
        .Select(c => new ActiveCheckInDto(
            c.Id, c.PatientId, c.Patient.FirstName, c.Patient.MrNumber, c.Patient.PatientType.Name,
            c.Doctor != null ? c.Doctor.FullName : null, c.Status.ToString(), c.CheckedInAtUtc))
        .ToListAsync();
    return Results.Ok(items);
}).RequireAuthorization();

app.MapGet("/api/consultations/{checkInId:guid}", async (Guid checkInId, AppDbContext db) =>
{
    var consultation = await db.Consultations
        .Include(c => c.Entries)
        .Include(c => c.Prescriptions)
        .Include(c => c.Patient)
        .FirstOrDefaultAsync(c => c.CheckInId == checkInId);

    if (consultation is null)
    {
        var checkIn = await db.CheckIns.Include(c => c.Patient).Include(c => c.Doctor)
            .FirstOrDefaultAsync(c => c.Id == checkInId);
        if (checkIn is null) return Results.NotFound();
        consultation = new Consultation
        {
            CheckInId = checkInId,
            PatientId = checkIn.PatientId,
            DoctorId = checkIn.DoctorId ?? (await db.Doctors.FirstAsync()).Id,
            Status = ConsultationStatus.InProgress
        };
        db.Consultations.Add(consultation);
        checkIn.Status = CheckInStatus.InConsultation;
        await db.SaveChangesAsync();
    }

    return Results.Ok(new ConsultationDto(
        consultation.Id,
        consultation.CheckInId,
        consultation.Patient.FirstName,
        consultation.Status.ToString(),
        consultation.ReferToCategory,
        consultation.Entries.Select(e => new ConsultationEntryDto(e.Id, e.SectionType.ToString(), e.Content, e.RecordedAtUtc)).ToList(),
        consultation.Prescriptions.Select(p => new PrescriptionDto(p.Id, p.MedicineName, p.Dosage, p.Strength)).ToList()));
}).RequireAuthorization();

app.MapPost("/api/consultations/{checkInId:guid}", async (Guid checkInId, SaveConsultationRequest req, AppDbContext db) =>
{
    var consultation = await db.Consultations
        .Include(c => c.Entries)
        .Include(c => c.Prescriptions)
        .Include(c => c.CheckIn)
        .FirstOrDefaultAsync(c => c.CheckInId == checkInId);

    if (consultation is null) return Results.NotFound();

    consultation.ReferToCategory = req.ReferToCategory;
    consultation.Status = req.Action switch
    {
        "Hold" => ConsultationStatus.OnHold,
        "Refer" => ConsultationStatus.Referred,
        _ => ConsultationStatus.Completed
    };
    consultation.CompletedAtUtc = consultation.Status == ConsultationStatus.Completed ? DateTime.UtcNow : null;
    consultation.CheckIn.Status = req.Action switch
    {
        "Hold" => CheckInStatus.OnHold,
        "Refer" => CheckInStatus.Referred,
        _ => CheckInStatus.Completed
    };

    db.ConsultationEntries.RemoveRange(consultation.Entries);
    consultation.Entries = req.Entries.Select(e => new ConsultationEntry
    {
        SectionType = Enum.Parse<ConsultationSectionType>(e.SectionType, true),
        Content = e.Content,
        DoctorId = consultation.DoctorId
    }).ToList();

    db.ConsultationPrescriptions.RemoveRange(consultation.Prescriptions);
    consultation.Prescriptions = req.Prescriptions.Select(p => new ConsultationPrescription
    {
        MedicineId = p.MedicineId,
        MedicineName = p.MedicineName,
        Dosage = p.Dosage,
        Route = p.Route,
        Strength = p.Strength
    }).ToList();

    await db.SaveChangesAsync();
    return Results.Ok(new { consultation.Id, consultation.Status });
}).RequireAuthorization();

app.MapGet("/api/medicines", async (string? search, AppDbContext db) =>
{
    var query = db.Medicines.Include(m => m.Category).Include(m => m.Generic).Include(m => m.Route).AsQueryable();
    if (!string.IsNullOrWhiteSpace(search))
        query = query.Where(m => m.Name.Contains(search) || (m.Generic != null && m.Generic.Name.Contains(search)));

    var meds = await query.OrderBy(m => m.Name).Take(100).Select(m => new MedicineDto(
        m.Id, m.Name, m.Category != null ? m.Category.Name : "", m.Generic != null ? m.Generic.Name : "",
        m.DosageSchedule, m.Route != null ? m.Route.Name : "", m.Strength, m.StockQuantity)).ToListAsync();
    return Results.Ok(meds);
}).RequireAuthorization();

app.MapGet("/api/reports/staff-performance", async (DateTime? from, DateTime? to, AppDbContext db) =>
{
    var start = from ?? DateTime.UtcNow.Date.AddDays(-7);
    var end = (to ?? DateTime.UtcNow.Date).AddDays(1);

    var users = await db.Users.Include(u => u.Role).Where(u => u.IsActive).ToListAsync();
    var rows = new List<StaffPerformanceRow>();

    foreach (var u in users)
    {
        var registered = await db.Patients.CountAsync(p => p.CreatedByUserId == u.Id && p.CreatedAtUtc >= start && p.CreatedAtUtc < end);
        var checkIns = await db.CheckIns.CountAsync(c => c.CheckedInByUserId == u.Id && c.CheckedInAtUtc >= start && c.CheckedInAtUtc < end);
        var services = await db.CheckInServiceLines.CountAsync(l => l.CheckIn.CheckedInByUserId == u.Id && l.CheckIn.CheckedInAtUtc >= start && l.CheckIn.CheckedInAtUtc < end);
        rows.Add(new StaffPerformanceRow(u.DisplayName, registered, checkIns, 0, 0, services, 0, registered + checkIns));
    }

    return Results.Ok(new StaffPerformanceReport(start, end, rows));
}).RequireAuthorization();

app.MapGet("/api/reports/filters", async (AppDbContext db) =>
{
    var services = await db.CatalogServices.Where(s => s.IsActive && s.IsDiagnostic)
        .OrderBy(s => s.Name)
        .Select(s => new FilterOptionDto(s.Id, s.Name)).ToListAsync();
    var doctors = await db.Doctors.Where(d => d.IsActive)
        .OrderBy(d => d.FullName)
        .Select(d => new FilterOptionDto(d.Id, d.FullName)).ToListAsync();
    var departments = await db.Departments
        .OrderBy(d => d.Name)
        .Select(d => new FilterOptionDto(d.Id, d.Name)).ToListAsync();
    var patientTypes = await db.PatientTypes
        .OrderBy(t => t.Name)
        .Select(t => new FilterOptionDto(t.Id, t.Name, t.Code)).ToListAsync();
    var stores = new List<FilterOptionDto> { new(Guid.Empty, "Main Pharmacy") };

    return Results.Ok(new ReportFiltersDto(services, doctors, departments, patientTypes, stores));
}).RequireAuthorization();

app.MapGet("/api/reports/lab-cash-flow", async (
    DateTime? from,
    DateTime? to,
    Guid? serviceId,
    Guid? doctorId,
    Guid? departmentId,
    Guid? patientTypeId,
    string? orderType,
    string? reportType,
    AppDbContext db) =>
{
    var start = from ?? DateTime.UtcNow.Date.AddDays(-7);
    var end = (to ?? DateTime.UtcNow.Date).AddDays(1);
    var summary = string.Equals(reportType, "Summary", StringComparison.OrdinalIgnoreCase);

    var query = db.CheckInServiceLines
        .Include(l => l.CheckIn).ThenInclude(c => c.Patient).ThenInclude(p => p.PatientType)
        .Include(l => l.CheckIn).ThenInclude(c => c.Doctor)
        .Include(l => l.CatalogService).ThenInclude(s => s.ServiceGroup)
        .Where(l => l.CatalogService.IsDiagnostic)
        .Where(l => l.CheckIn.CheckedInAtUtc >= start && l.CheckIn.CheckedInAtUtc < end);

    if (serviceId.HasValue)
        query = query.Where(l => l.CatalogServiceId == serviceId.Value);
    if (doctorId.HasValue)
        query = query.Where(l => l.CheckIn.DoctorId == doctorId.Value);
    if (departmentId.HasValue)
        query = query.Where(l => l.CheckIn.Doctor != null && l.CheckIn.Doctor.DepartmentId == departmentId.Value);
    if (patientTypeId.HasValue)
        query = query.Where(l => l.CheckIn.Patient.PatientTypeId == patientTypeId.Value);

    if (string.Equals(orderType, "LAB", StringComparison.OrdinalIgnoreCase))
        query = query.Where(l => l.CheckIn.Destination == CheckInDestination.InvestigationsDiagnostics);
    else if (string.Equals(orderType, "OPD", StringComparison.OrdinalIgnoreCase))
        query = query.Where(l => l.CheckIn.Destination == CheckInDestination.Doctor);

    var lines = await query.OrderByDescending(l => l.CheckIn.CheckedInAtUtc).ToListAsync();

    if (summary)
    {
        var rows = lines
            .GroupBy(l => l.ServiceName)
            .Select(g => new LabCashFlowSummaryRow(g.Key, g.Count(), g.Sum(x => x.Charges)))
            .OrderByDescending(r => r.TotalCharges)
            .ToList();
        return Results.Ok(new LabCashFlowReport(start, end, "Summary", null, rows, rows.Sum(r => r.TotalCharges)));
    }

    var detailed = lines.Select(l => new LabCashFlowDetailRow(
        l.CheckIn.CheckedInAtUtc,
        l.CheckIn.Patient.FirstName,
        l.CheckIn.Patient.MrNumber,
        l.ServiceName,
        l.CheckIn.Doctor?.FullName,
        l.CheckIn.Patient.PatientType.Name,
        l.Charges)).ToList();

    return Results.Ok(new LabCashFlowReport(start, end, "Detailed", detailed, null, detailed.Sum(r => r.Charges)));
}).RequireAuthorization();

app.MapGet("/api/reports/pharmacy-census", async (DateTime? from, DateTime? to, AppDbContext db) =>
{
    var start = from ?? DateTime.UtcNow.Date.AddDays(-7);
    var end = (to ?? DateTime.UtcNow.Date).AddDays(1);

    var consultations = await db.Consultations
        .Include(c => c.Patient)
        .Include(c => c.Prescriptions)
        .Where(c => c.Status == ConsultationStatus.Completed)
        .Where(c => c.CompletedAtUtc >= start && c.CompletedAtUtc < end)
        .Where(c => c.Prescriptions.Any())
        .ToListAsync();

    var rows = consultations
        .GroupBy(c => DateOnly.FromDateTime(c.CompletedAtUtc!.Value))
        .OrderBy(g => g.Key)
        .Select(g =>
        {
            var males = g.Count(c => c.Patient.Gender == PatientGender.Male);
            var females = g.Count(c => c.Patient.Gender == PatientGender.Female);
            return new PharmacyCensusRow(g.Key, males, females, males + females);
        })
        .ToList();

    return Results.Ok(new PharmacyCensusReport(start, end, rows));
}).RequireAuthorization();

app.MapGet("/api/panels", async (AppDbContext db) =>
{
    var panels = await db.CorporatePanels.Where(p => p.IsActive)
        .OrderBy(p => p.Name)
        .Select(p => new PanelDto(p.Id, p.Code, p.Name)).ToListAsync();
    return Results.Ok(panels);
}).RequireAuthorization();

app.MapGet("/api/lab/orders", async (string? status, AppDbContext db) =>
{
    var query = db.LabWorkItems
        .Include(l => l.CheckInServiceLine).ThenInclude(s => s.CheckIn).ThenInclude(c => c.Patient)
        .Include(l => l.CheckInServiceLine).ThenInclude(s => s.CatalogService)
        .AsQueryable();

    if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<LabWorkStatus>(status, true, out var st))
        query = query.Where(l => l.Status == st);

    var items = await query.OrderByDescending(l => l.CreatedAtUtc).Take(100)
        .Select(l => new LabOrderDto(
            l.Id,
            l.CheckInServiceLine.CheckIn.Patient.FirstName,
            l.CheckInServiceLine.CheckIn.Patient.MrNumber,
            l.CheckInServiceLine.ServiceName,
            l.CheckInServiceLine.Charges,
            l.Status.ToString(),
            l.CheckInServiceLine.CheckIn.CheckedInAtUtc))
        .ToListAsync();
    return Results.Ok(items);
}).RequireAuthorization();

app.MapPost("/api/lab/orders/{id:guid}/verify-payment", async (Guid id, AppDbContext db, ClaimsPrincipal user) =>
{
    var item = await db.LabWorkItems.FindAsync(id);
    if (item is null) return Results.NotFound();
    if (item.Status != LabWorkStatus.PendingPayment)
        return Results.BadRequest(new { message = "Order is not pending payment." });

    item.Status = LabWorkStatus.PaymentVerified;
    item.PaymentVerifiedAtUtc = DateTime.UtcNow;
    item.PaymentVerifiedByUserId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
    await db.SaveChangesAsync();
    return Results.Ok(new { item.Id, item.Status });
}).RequireAuthorization();

app.MapPost("/api/lab/orders/{id:guid}/collect-sample", async (Guid id, AppDbContext db, ClaimsPrincipal user) =>
{
    var item = await db.LabWorkItems.FindAsync(id);
    if (item is null) return Results.NotFound();
    if (item.Status != LabWorkStatus.PaymentVerified)
        return Results.BadRequest(new { message = "Payment must be verified first." });

    item.Status = LabWorkStatus.SampleCollected;
    item.SampleCollectedAtUtc = DateTime.UtcNow;
    item.SampleCollectedByUserId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
    await db.SaveChangesAsync();
    return Results.Ok(new { item.Id, item.Status });
}).RequireAuthorization();

app.MapGet("/api/reports/services-cash-flow", async (DateTime? from, DateTime? to, Guid? serviceId, AppDbContext db) =>
{
    var start = from ?? DateTime.UtcNow.Date.AddDays(-7);
    var end = (to ?? DateTime.UtcNow.Date).AddDays(1);

    var query = db.CheckInServiceLines
        .Include(l => l.CheckIn).ThenInclude(c => c.Patient).ThenInclude(p => p.PatientType)
        .Include(l => l.CatalogService)
        .Where(l => l.CheckIn.CheckedInAtUtc >= start && l.CheckIn.CheckedInAtUtc < end);

    if (serviceId.HasValue)
        query = query.Where(l => l.CatalogServiceId == serviceId.Value);

    var lines = await query.OrderByDescending(l => l.CheckIn.CheckedInAtUtc).ToListAsync();
    var rows = lines.Select(l => new ServicesCashFlowRow(
        l.CheckIn.CheckedInAtUtc, l.ServiceName, l.CheckIn.Patient.FirstName,
        l.CheckIn.Patient.PatientType.Name, l.Charges)).ToList();

    return Results.Ok(new ServicesCashFlowReport(start, end, rows, rows.Sum(r => r.Charges)));
}).RequireAuthorization();

app.MapGet("/api/reports/reception-cash-flow", async (DateTime? from, DateTime? to, AppDbContext db) =>
{
    var start = from ?? DateTime.UtcNow.Date.AddDays(-7);
    var end = (to ?? DateTime.UtcNow.Date).AddDays(1);

    var challans = await db.Challans
        .Include(c => c.IssuedByUser)
        .Include(c => c.CheckIn).ThenInclude(ci => ci.Patient)
        .Where(c => c.IssuedAtUtc >= start && c.IssuedAtUtc < end)
        .OrderByDescending(c => c.IssuedAtUtc)
        .ToListAsync();

    var rows = challans.Select(c => new ReceptionCashFlowRow(
        c.IssuedAtUtc,
        c.IssuedByUser.DisplayName,
        c.CheckIn.Patient.FirstName,
        c.ChallanNumber,
        c.PatientPayable,
        c.PanelPayable,
        c.SubTotal)).ToList();

    return Results.Ok(new ReceptionCashFlowReport(start, end, rows, rows.Sum(r => r.PatientCollected), rows.Sum(r => r.PanelAmount)));
}).RequireAuthorization();

app.MapGet("/api/branches", async (AppDbContext db) =>
{
    var branches = await db.Branches.Where(b => b.IsActive)
        .OrderBy(b => b.Name)
        .Select(b => new BranchDto(b.Id, b.Code, b.Name, b.Region, b.IsHeadOffice))
        .ToListAsync();
    return Results.Ok(branches);
}).RequireAuthorization();

app.MapGet("/api/imaging/orders", async (AppDbContext db) =>
{
    var items = await db.CheckInServiceLines
        .Include(l => l.CheckIn).ThenInclude(c => c.Patient)
        .Include(l => l.CatalogService)
        .Where(l => l.CatalogService.IsImaging)
        .OrderByDescending(l => l.CheckIn.CheckedInAtUtc)
        .Take(100)
        .Select(l => new ImagingOrderDto(
            l.Id,
            l.CheckIn.Patient.FirstName,
            l.CheckIn.Patient.MrNumber,
            l.ServiceName,
            l.Charges,
            l.DeliveryDate,
            l.CheckIn.Status.ToString(),
            l.CheckIn.CheckedInAtUtc))
        .ToListAsync();
    return Results.Ok(items);
}).RequireAuthorization();

app.MapGet("/api/reports/region-wise", async (DateTime? from, DateTime? to, AppDbContext db) =>
{
    var start = from ?? DateTime.UtcNow.Date.AddDays(-30);
    var end = (to ?? DateTime.UtcNow.Date).AddDays(1);

    var branches = await db.Branches.Where(b => b.IsActive).ToListAsync();
    var rows = new List<RegionWiseRow>();

    foreach (var branch in branches)
    {
        var registered = await db.Patients.CountAsync(p => p.BranchId == branch.Id && p.CreatedAtUtc >= start && p.CreatedAtUtc < end);
        var opd = await db.CheckIns.CountAsync(c => c.BranchId == branch.Id && c.CheckedInAtUtc >= start && c.CheckedInAtUtc < end);
        var revenue = await db.Challans.Where(c => c.BranchId == branch.Id && c.IssuedAtUtc >= start && c.IssuedAtUtc < end)
            .SumAsync(c => c.SubTotal);
        rows.Add(new RegionWiseRow(branch.Region ?? branch.Name, branch.Name, registered, opd, revenue));
    }

    return Results.Ok(new RegionWiseReport(start, end, rows.OrderByDescending(r => r.Revenue).ToList()));
}).RequireAuthorization();

app.MapGet("/api/reports/average-opd", async (DateTime? from, DateTime? to, AppDbContext db) =>
{
    var start = from ?? DateTime.UtcNow.Date.AddDays(-30);
    var end = (to ?? DateTime.UtcNow.Date).AddDays(1);

    var checkIns = await db.CheckIns
        .Where(c => c.CheckedInAtUtc >= start && c.CheckedInAtUtc < end)
        .ToListAsync();

    var daily = checkIns
        .GroupBy(c => DateOnly.FromDateTime(c.CheckedInAtUtc))
        .OrderBy(g => g.Key)
        .Select(g => new AverageOpdDailyRow(g.Key, g.Count()))
        .ToList();

    var dayCount = Math.Max(1, (end.Date - start.Date).Days);
    var totalOpd = checkIns.Count;
    var avgPerDay = Math.Round((double)totalOpd / dayCount, 1);
    var peakDay = daily.OrderByDescending(d => d.OpdCount).FirstOrDefault();

    return Results.Ok(new AverageOpdReport(start, end, daily, totalOpd, avgPerDay, peakDay?.OpdCount ?? 0, peakDay?.Date));
}).RequireAuthorization();

app.MapGet("/api/challans", async (string? search, DateTime? from, DateTime? to, AppDbContext db, ClaimsPrincipal user) =>
{
    var branchId = Guid.Parse(user.FindFirstValue("branchId")!);
    var start = from ?? DateTime.UtcNow.Date.AddDays(-30);
    var end = (to ?? DateTime.UtcNow.Date).AddDays(1);

    var query = db.Challans
        .Include(c => c.IssuedByUser)
        .Include(c => c.CheckIn).ThenInclude(ci => ci.Patient)
        .Include(c => c.CheckIn).ThenInclude(ci => ci.ServiceLines)
        .Where(c => c.BranchId == branchId && c.IssuedAtUtc >= start && c.IssuedAtUtc < end);

    if (!string.IsNullOrWhiteSpace(search))
    {
        var term = search.Trim();
        query = query.Where(c =>
            c.ChallanNumber.Contains(term) ||
            c.CheckIn.Patient.FirstName.Contains(term) ||
            c.CheckIn.Patient.MrNumber.Contains(term));
    }

    var items = await query
        .OrderByDescending(c => c.IssuedAtUtc)
        .Take(100)
        .Select(c => new ChallanVaultDto(
            c.Id,
            c.ChallanNumber,
            c.CheckIn.Patient.FirstName,
            c.CheckIn.Patient.MrNumber,
            string.Join(", ", c.CheckIn.ServiceLines.Select(l => l.ServiceName)),
            c.SubTotal,
            c.PatientPayable,
            c.PanelPayable,
            c.IssuedAtUtc,
            c.IssuedByUser.DisplayName))
        .ToListAsync();

    return Results.Ok(items);
}).RequireAuthorization();

app.Run();

record LoginRequest(string UserName, string Password);
record LoginResponse(string Token, string DisplayName, string RoleCode, string RoleName);
record CreatePatientRequest(string FirstName, string GuardianName, string Gender, string CnicOrPassport, string PatientTypeCode, string? Mobile, string? Address);
record PatientDto(Guid Id, string FirstName, string GuardianName, string MrNumber, string CnicOrPassport, string Gender, string PatientType, string? Mobile, DateTime RegisteredOn, Guid? CorporatePanelId, string? CorporatePanelName, string BranchName, string? BranchRegion);
record DoctorDto(Guid Id, string FullName, string Specialty);
record ServiceGroupDto(Guid Id, string Name, List<ServiceDto> Services);
record ServiceDto(Guid Id, string Name, decimal Charges, int? DeliveryDays);
record CreateCheckInRequest(Guid PatientId, Guid? DoctorId, string CheckInType, string Destination, string PrescribedBy, string SmsAlert, string? PackageName, string? PanelBillingMode, List<CheckInLineRequest> ServiceLines);
record CheckInLineRequest(Guid ServiceId, string ServiceName, decimal Charges, DateOnly? DeliveryDate);
record CheckInResultDto(Guid CheckInId, string ChallanNumber, decimal SubTotal, decimal PatientPayable, decimal PanelPayable, string? PanelBillingMode);
record ActiveCheckInDto(Guid Id, Guid PatientId, string PatientName, string MrNumber, string PatientType, string? DoctorName, string Status, DateTime CheckedInAt);
record ConsultationDto(Guid Id, Guid CheckInId, string PatientName, string Status, bool ReferToCategory, List<ConsultationEntryDto> Entries, List<PrescriptionDto> Prescriptions);
record ConsultationEntryDto(Guid Id, string SectionType, string Content, DateTime RecordedAt);
record PrescriptionDto(Guid Id, string MedicineName, string? Dosage, string? Strength);
record SaveConsultationRequest(bool ReferToCategory, string Action, List<EntryInput> Entries, List<PrescriptionInput> Prescriptions);
record EntryInput(string SectionType, string Content);
record PrescriptionInput(Guid? MedicineId, string MedicineName, string? Dosage, string? Route, string? Strength);
record MedicineDto(Guid Id, string Name, string Category, string Generic, string? Dosage, string? Route, string? Strength, decimal Stock);
record StaffPerformanceRow(string UserName, int Registered, int Opd, int Er, int Ipd, int Services, int Doctor, int VisitedTotal);
record StaffPerformanceReport(DateTime From, DateTime To, List<StaffPerformanceRow> Rows);
record FilterOptionDto(Guid Id, string Name, string? Code = null);
record ReportFiltersDto(List<FilterOptionDto> Services, List<FilterOptionDto> Doctors, List<FilterOptionDto> Departments, List<FilterOptionDto> PatientTypes, List<FilterOptionDto> Stores);
record LabCashFlowDetailRow(DateTime Date, string PatientName, string MrNumber, string ServiceName, string? DoctorName, string PatientType, decimal Charges);
record LabCashFlowSummaryRow(string ServiceName, int Count, decimal TotalCharges);
record LabCashFlowReport(DateTime From, DateTime To, string ReportType, List<LabCashFlowDetailRow>? DetailedRows, List<LabCashFlowSummaryRow>? SummaryRows, decimal GrandTotal);
record PharmacyCensusRow(DateOnly Date, int Males, int Females, int Total);
record PharmacyCensusReport(DateTime From, DateTime To, List<PharmacyCensusRow> Rows);
record PanelDto(Guid Id, string Code, string Name);
record LabOrderDto(Guid Id, string PatientName, string MrNumber, string ServiceName, decimal Charges, string Status, DateTime OrderedAt);
record ServicesCashFlowRow(DateTime Date, string ServiceName, string PatientName, string PatientType, decimal Charges);
record ServicesCashFlowReport(DateTime From, DateTime To, List<ServicesCashFlowRow> Rows, decimal GrandTotal);
record ReceptionCashFlowRow(DateTime Date, string CollectedBy, string PatientName, string ChallanNumber, decimal PatientCollected, decimal PanelAmount, decimal SubTotal);
record ReceptionCashFlowReport(DateTime From, DateTime To, List<ReceptionCashFlowRow> Rows, decimal TotalPatientCollected, decimal TotalPanelAmount);
record BranchDto(Guid Id, string Code, string Name, string? Region, bool IsHeadOffice);
record ImagingOrderDto(Guid Id, string PatientName, string MrNumber, string ServiceName, decimal Charges, DateOnly? DeliveryDate, string Status, DateTime OrderedAt);
record RegionWiseRow(string Region, string BranchName, int Registered, int Opd, decimal Revenue);
record RegionWiseReport(DateTime From, DateTime To, List<RegionWiseRow> Rows);
record AverageOpdDailyRow(DateOnly Date, int OpdCount);
record AverageOpdReport(DateTime From, DateTime To, List<AverageOpdDailyRow> DailyRows, int TotalOpd, double AverageOpdPerDay, int PeakDayCount, DateOnly? PeakDate);
record ChallanVaultDto(Guid Id, string ChallanNumber, string PatientName, string MrNumber, string Services, decimal SubTotal, decimal PatientPayable, decimal PanelPayable, DateTime IssuedAt, string IssuedBy);
