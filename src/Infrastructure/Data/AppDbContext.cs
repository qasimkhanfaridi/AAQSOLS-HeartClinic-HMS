using HeartClinicHms.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HeartClinicHms.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<PatientType> PatientTypes => Set<PatientType>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<MrNumberSequence> MrNumberSequences => Set<MrNumberSequence>();
    public DbSet<ServiceGroup> ServiceGroups => Set<ServiceGroup>();
    public DbSet<CatalogService> CatalogServices => Set<CatalogService>();
    public DbSet<CheckIn> CheckIns => Set<CheckIn>();
    public DbSet<CheckInServiceLine> CheckInServiceLines => Set<CheckInServiceLine>();
    public DbSet<Challan> Challans => Set<Challan>();
    public DbSet<Consultation> Consultations => Set<Consultation>();
    public DbSet<ConsultationEntry> ConsultationEntries => Set<ConsultationEntry>();
    public DbSet<ConsultationPrescription> ConsultationPrescriptions => Set<ConsultationPrescription>();
    public DbSet<MedicineCategory> MedicineCategories => Set<MedicineCategory>();
    public DbSet<MedicineGeneric> MedicineGenerics => Set<MedicineGeneric>();
    public DbSet<MedicineDosageForm> MedicineDosageForms => Set<MedicineDosageForm>();
    public DbSet<MedicineRoute> MedicineRoutes => Set<MedicineRoute>();
    public DbSet<Medicine> Medicines => Set<Medicine>();
    public DbSet<CorporatePanel> CorporatePanels => Set<CorporatePanel>();
    public DbSet<PanelServiceRate> PanelServiceRates => Set<PanelServiceRate>();
    public DbSet<LabWorkItem> LabWorkItems => Set<LabWorkItem>();
    public DbSet<ImportBatch> ImportBatches => Set<ImportBatch>();
    public DbSet<ImportMappingLog> ImportMappingLogs => Set<ImportMappingLog>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
