using HeartClinicHms.Domain.Entities;
using HeartClinicHms.Domain.Enums;
using HeartClinicHms.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HeartClinicHms.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (!await db.Branches.AnyAsync())
            await SeedFoundationAsync(db);

        if (!await db.CheckIns.AnyAsync())
            await SeedDemoActivityAsync(db);

        await SeedPanelAndLabRetrofitAsync(db);
        await SeedPhase3MultiBranchAsync(db);
    }

    static async Task SeedPanelAndLabRetrofitAsync(AppDbContext db)
    {
        if (!await db.CorporatePanels.AnyAsync())
        {
            var branch = await db.Branches.FirstAsync();
            var holter = await db.CatalogServices.FirstAsync(s => s.Code == "HOLTER");
            var ecg = await db.CatalogServices.FirstAsync(s => s.Code == "ECG");

            var panel = new CorporatePanel
            {
                BranchId = branch.Id,
                Code = "ENGRO",
                Name = "Engro Corporation Panel",
                ContactPerson = "HR Benefits"
            };
            db.CorporatePanels.Add(panel);
            db.PanelServiceRates.AddRange(
                new PanelServiceRate { CorporatePanel = panel, CatalogServiceId = holter.Id, PanelRate = 7000 },
                new PanelServiceRate { CorporatePanel = panel, CatalogServiceId = ecg.Id, PanelRate = 1200 });
            await db.SaveChangesAsync();

            var sara = await db.Patients.FirstOrDefaultAsync(p => p.MrNumber == "5601-26-001210");
            if (sara is not null)
            {
                sara.CorporatePanelId = panel.Id;
                await db.SaveChangesAsync();
            }
        }

        var diagnosticLines = await db.CheckInServiceLines
            .Include(l => l.CatalogService)
            .Include(l => l.LabWorkItem)
            .Where(l => l.CatalogService.IsDiagnostic && l.LabWorkItem == null)
            .ToListAsync();

        foreach (var line in diagnosticLines)
            db.LabWorkItems.Add(new LabWorkItem { CheckInServiceLineId = line.Id, Status = LabWorkStatus.PendingPayment });

        var challansMissingSplit = await db.Challans.Where(c => c.PatientPayable == 0 && c.SubTotal > 0).ToListAsync();
        foreach (var c in challansMissingSplit)
        {
            c.PatientPayable = c.SubTotal;
            c.PanelPayable = 0;
        }

        if (diagnosticLines.Count > 0 || challansMissingSplit.Count > 0)
            await db.SaveChangesAsync();
    }

    static async Task SeedPhase3MultiBranchAsync(AppDbContext db)
    {
        var main = await db.Branches.FirstAsync(b => b.Code == "THC-MAIN");
        if (string.IsNullOrEmpty(main.Region))
        {
            main.Region = "Rawalpindi";
            await db.SaveChangesAsync();
        }

        if (await db.Branches.CountAsync() < 2)
        {
            var isbBranch = new Branch
            {
                Code = "THC-ISB",
                Name = "The Heart Clinic — Islamabad",
                Address = "F-8 Markaz, Islamabad",
                Region = "Islamabad",
                IsHeadOffice = false
            };
            db.Branches.Add(isbBranch);

            var privateType = await db.PatientTypes.FirstAsync(t => t.Code == "PRIVATE");
            db.Patients.Add(new Patient
            {
                Branch = isbBranch,
                PatientType = privateType,
                MrNumber = "5602-26-000101",
                FirstName = "Mr. Islamabad Patient",
                GuardianName = "Khan",
                Gender = PatientGender.Male,
                CnicOrPassport = "61101-1234567-1",
                Mobile = "0300-5550001"
            });
            await db.SaveChangesAsync();
        }

        if (!await db.CatalogServices.AnyAsync(s => s.Code == "ECHO2D"))
        {
            var mainBranch = await db.Branches.FirstAsync(b => b.Code == "THC-MAIN");
            var imagingGroup = new ServiceGroup { Name = "Imaging", SortOrder = 2, Branch = mainBranch };
            db.ServiceGroups.Add(imagingGroup);
            db.CatalogServices.Add(new CatalogService
            {
                Name = "2D Echo Cardiography",
                Code = "ECHO2D",
                DefaultCharges = 5500,
                DefaultDeliveryDays = 1,
                IsImaging = true,
                Branch = mainBranch,
                ServiceGroup = imagingGroup
            });
            await db.SaveChangesAsync();
        }

        var holter = await db.CatalogServices.FirstOrDefaultAsync(s => s.Code == "HOLTER");
        if (holter is not null && !holter.IsImaging)
        {
            holter.IsImaging = true;
            await db.SaveChangesAsync();
        }
    }

    static async Task SeedFoundationAsync(AppDbContext db)
    {
        var hasher = new PasswordHasher<User>();

        var branch = new Branch
        {
            Code = "THC-MAIN",
            Name = "AAQSOLS Heart Clinic",
            IsHeadOffice = true,
            Address = "Rawalpindi",
            Region = "Rawalpindi"
        };

        var adminRole = new Role { Code = "ADMIN", Name = "Administrator" };
        var receptionRole = new Role { Code = "RECEPTION", Name = "Reception" };
        var doctorRole = new Role { Code = "DOCTOR", Name = "Doctor" };

        var privateType = new PatientType { Code = "PRIVATE", Name = "Private" };
        var panelType = new PatientType { Code = "PANEL", Name = "Panel" };

        var cardiologyDept = new Department { Name = "Cardiology", Code = "CARD", Branch = branch };

        var adminUser = new User
        {
            UserName = "admin",
            Email = "admin@aaqsols.com",
            DisplayName = "Mr. M Kabir",
            AvatarInitial = "M",
            Role = adminRole,
            Branch = branch
        };
        adminUser.PasswordHash = hasher.HashPassword(adminUser, "Admin@123");

        var receptionUser = new User
        {
            UserName = "reception",
            Email = "reception@aaqsols.com",
            DisplayName = "Reception Desk",
            AvatarInitial = "R",
            Role = receptionRole,
            Branch = branch
        };
        receptionUser.PasswordHash = hasher.HashPassword(receptionUser, "Reception@123");

        var doctorUser = new User
        {
            UserName = "doctor",
            Email = "doctor@aaqsols.com",
            DisplayName = "Dr. M Abdus Salam Azad",
            AvatarInitial = "A",
            Role = doctorRole,
            Branch = branch
        };
        doctorUser.PasswordHash = hasher.HashPassword(doctorUser, "Doctor@123");

        var doctorMalik = new Doctor
        {
            FullName = "Dr. Abdul Malik",
            Specialty = "Consultant Cardiologist",
            Branch = branch,
            Department = cardiologyDept
        };

        var doctorAzad = new Doctor
        {
            FullName = "Dr. M Abdus Salam Azad",
            Specialty = "Consultant Cardiologist",
            Branch = branch,
            Department = cardiologyDept,
            User = doctorUser
        };

        var holterGroup = new ServiceGroup { Name = "Diagnostics", SortOrder = 1, Branch = branch };
        var holterService = new CatalogService
        {
            Name = "24-48 Hr. Holter Monitor",
            Code = "HOLTER",
            DefaultCharges = 8500,
            DefaultDeliveryDays = 2,
            IsDiagnostic = true,
            Branch = branch,
            ServiceGroup = holterGroup
        };
        var ecgService = new CatalogService
        {
            Name = "ECG",
            Code = "ECG",
            DefaultCharges = 1500,
            IsDiagnostic = true,
            Branch = branch,
            ServiceGroup = holterGroup
        };

        var cardiologyCategory = new MedicineCategory { Name = "Cardiology" };
        var lipolite = new MedicineGeneric { Name = "Atorvastatin" };
        var adrance = new MedicineGeneric { Name = "Empagliflozin + Linagliptin" };

        var tabRoute = new MedicineRoute { Name = "mouth" };
        var capsuleForm = new MedicineDosageForm { Name = "Tablet" };

        var medicines = new List<Medicine>
        {
            new() { Name = "Tab Lipolite 10mg", Category = cardiologyCategory, Generic = lipolite, Route = tabRoute, DosageForm = capsuleForm, DosageSchedule = "once daily", Strength = "10mg" },
            new() { Name = "Tab Adrance-L (Empagliflozin + Linagliptin) 25mg/5mg", Category = cardiologyCategory, Generic = adrance, Route = tabRoute, DosageForm = capsuleForm, DosageSchedule = "once daily", Strength = "25mg/5mg" },
            new() { Name = "Tab Valam-H 160/12.5mg", Category = cardiologyCategory, Generic = new MedicineGeneric { Name = "Valsartan + Hydrochlorothiazide" }, Route = tabRoute, DosageForm = capsuleForm, DosageSchedule = "once daily", Strength = "160/12.5mg" }
        };

        var patients = new List<Patient>
        {
            new()
            {
                Branch = branch,
                PatientType = privateType,
                MrNumber = "5601-26-001212",
                FirstName = "Mr. Nisar Ahmed",
                GuardianName = "Ahmed Khan",
                Gender = PatientGender.Male,
                CnicOrPassport = "37405-9227187-1",
                Mobile = "0300-1234567"
            },
            new()
            {
                Branch = branch,
                PatientType = privateType,
                MrNumber = "5601-26-001211",
                FirstName = "Mr. Demo Patient",
                GuardianName = "Demo Guardian",
                Gender = PatientGender.Male,
                CnicOrPassport = "35202-1234567-9",
                Mobile = "0301-9876543"
            },
            new()
            {
                Branch = branch,
                PatientType = panelType,
                MrNumber = "5601-26-001210",
                FirstName = "Mrs. Sara Khan",
                GuardianName = "Ali Khan",
                Gender = PatientGender.Female,
                CnicOrPassport = "35202-9876543-2",
                Mobile = "0333-5551234"
            }
        };

        db.AddRange(branch, adminRole, receptionRole, doctorRole, privateType, panelType, cardiologyDept);
        db.AddRange(adminUser, receptionUser, doctorUser);
        db.AddRange(doctorMalik, doctorAzad);
        db.AddRange(holterGroup, holterService, ecgService);
        db.AddRange(tabRoute, capsuleForm, cardiologyCategory, lipolite, adrance);
        db.Medicines.AddRange(medicines);
        db.Patients.AddRange(patients);
        db.MrNumberSequences.Add(new MrNumberSequence { Branch = branch, Year = 26, Prefix = 5601, LastSequence = 1212 });

        await db.SaveChangesAsync();
    }

    static async Task SeedDemoActivityAsync(AppDbContext db)
    {
        var branch = await db.Branches.FirstAsync();
        var reception = await db.Users.FirstAsync(u => u.UserName == "reception");
        var doctorMalik = await db.Doctors.FirstAsync(d => d.FullName.Contains("Malik"));
        var doctorAzad = await db.Doctors.FirstAsync(d => d.FullName.Contains("Azad"));
        var nisar = await db.Patients.FirstAsync(p => p.MrNumber == "5601-26-001212");
        var demo = await db.Patients.FirstAsync(p => p.MrNumber == "5601-26-001211");
        var sara = await db.Patients.FirstOrDefaultAsync(p => p.MrNumber == "5601-26-001210");
        if (sara is null)
        {
            var panelType = await db.PatientTypes.FirstAsync(t => t.Code == "PANEL");
            sara = new Patient
            {
                BranchId = branch.Id,
                PatientTypeId = panelType.Id,
                MrNumber = "5601-26-001210",
                FirstName = "Mrs. Sara Khan",
                GuardianName = "Ali Khan",
                Gender = PatientGender.Female,
                CnicOrPassport = "35202-9876543-2",
                Mobile = "0333-5551234"
            };
            db.Patients.Add(sara);
            await db.SaveChangesAsync();
        }
        var holter = await db.CatalogServices.FirstAsync(s => s.Code == "HOLTER");
        var ecg = await db.CatalogServices.FirstAsync(s => s.Code == "ECG");
        var lipolite = await db.Medicines.FirstAsync(m => m.Name.Contains("Lipolite"));
        var adrance = await db.Medicines.FirstAsync(m => m.Name.Contains("Adrance"));

        var today = DateTime.UtcNow.Date;

        await AddCheckIn(db, branch.Id, nisar, doctorMalik, reception.Id, holter, today.AddDays(-4), CheckInStatus.Completed);
        await AddCheckIn(db, branch.Id, demo, doctorAzad, reception.Id, ecg, today.AddDays(-3), CheckInStatus.Completed);
        await AddCheckIn(db, branch.Id, nisar, doctorMalik, reception.Id, ecg, today.AddDays(-2), CheckInStatus.Completed);
        await AddCheckIn(db, branch.Id, sara, doctorAzad, reception.Id, holter, today.AddDays(-1), CheckInStatus.Completed);
        await AddCheckIn(db, branch.Id, demo, doctorMalik, reception.Id, holter, today, CheckInStatus.CheckedIn);

        await db.SaveChangesAsync();

        var completedCheckIns = await db.CheckIns
            .Include(c => c.Patient)
            .Where(c => c.Status == CheckInStatus.Completed)
            .OrderBy(c => c.CheckedInAtUtc)
            .ToListAsync();

        foreach (var checkIn in completedCheckIns.Take(3))
        {
            var doctor = checkIn.DoctorId == doctorMalik.Id ? doctorMalik : doctorAzad;
            var consultation = new Consultation
            {
                CheckInId = checkIn.Id,
                PatientId = checkIn.PatientId,
                DoctorId = doctor.Id,
                Status = ConsultationStatus.Completed,
                CompletedAtUtc = checkIn.CheckedInAtUtc.AddHours(2),
                StartedAtUtc = checkIn.CheckedInAtUtc.AddHours(1)
            };
            consultation.Entries.Add(new ConsultationEntry
            {
                SectionType = ConsultationSectionType.Advice,
                Content = "Continue current medication. Follow up in 2 weeks.",
                DoctorId = doctor.Id,
                RecordedAtUtc = consultation.CompletedAtUtc.Value
            });
            consultation.Prescriptions.Add(new ConsultationPrescription
            {
                MedicineId = lipolite.Id,
                MedicineName = lipolite.Name,
                Dosage = lipolite.DosageSchedule,
                Strength = lipolite.Strength,
                Route = "mouth"
            });
            if (checkIn.Patient.Gender == PatientGender.Female)
            {
                consultation.Prescriptions.Add(new ConsultationPrescription
                {
                    MedicineId = adrance.Id,
                    MedicineName = adrance.Name,
                    Dosage = adrance.DosageSchedule,
                    Strength = adrance.Strength,
                    Route = "mouth"
                });
            }
            db.Consultations.Add(consultation);
        }

        await db.SaveChangesAsync();
    }

    static async Task AddCheckIn(
        AppDbContext db,
        Guid branchId,
        Patient patient,
        Doctor doctor,
        Guid receptionId,
        CatalogService service,
        DateTime checkedInAt,
        CheckInStatus status)
    {
        var checkIn = new CheckIn
        {
            BranchId = branchId,
            PatientId = patient.Id,
            DoctorId = doctor.Id,
            CheckedInByUserId = receptionId,
            CheckInType = CheckInType.WalkIn,
            Destination = CheckInDestination.InvestigationsDiagnostics,
            PrescribedBy = PrescribedBy.Doctor,
            SmsAlert = SmsAlertOption.Patient,
            Status = status,
            SubTotal = service.DefaultCharges,
            CheckedInAtUtc = checkedInAt.AddHours(10)
        };
        checkIn.ServiceLines.Add(new CheckInServiceLine
        {
            CatalogServiceId = service.Id,
            ServiceName = service.Name,
            Charges = service.DefaultCharges,
            DeliveryDate = service.DefaultDeliveryDays.HasValue
                ? DateOnly.FromDateTime(checkedInAt.AddDays(service.DefaultDeliveryDays.Value))
                : null
        });
        checkIn.Challan = new Challan
        {
            BranchId = branchId,
            ChallanNumber = $"CH-{checkedInAt:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}",
            SubTotal = service.DefaultCharges,
            PatientPayable = service.DefaultCharges,
            PanelPayable = 0,
            IssuedByUserId = receptionId,
            IssuedAtUtc = checkedInAt.AddHours(10)
        };
        db.CheckIns.Add(checkIn);
        await db.SaveChangesAsync();

        if (service.IsDiagnostic)
        {
            var line = checkIn.ServiceLines.First();
            db.LabWorkItems.Add(new LabWorkItem
            {
                CheckInServiceLineId = line.Id,
                Status = status == CheckInStatus.Completed ? LabWorkStatus.PaymentVerified : LabWorkStatus.PendingPayment
            });
        }
        await Task.CompletedTask;
    }
}
