using HeartClinicHms.Domain.Common;
using HeartClinicHms.Domain.Enums;

namespace HeartClinicHms.Domain.Entities;

public class ServiceGroup : MigratableEntity
{
    public Guid BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    public Branch Branch { get; set; } = null!;
    public ICollection<CatalogService> Services { get; set; } = [];
}

public class CatalogService : MigratableEntity
{
    public Guid BranchId { get; set; }
    public Guid? ServiceGroupId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public decimal DefaultCharges { get; set; }
    public int? DefaultDeliveryDays { get; set; }
    public bool IsDiagnostic { get; set; }
    public bool IsImaging { get; set; }

    public Branch Branch { get; set; } = null!;
    public ServiceGroup? ServiceGroup { get; set; }
    public ICollection<CheckInServiceLine> CheckInServiceLines { get; set; } = [];
}

public class CheckIn : MigratableEntity
{
    public Guid BranchId { get; set; }
    public Guid PatientId { get; set; }
    public Guid? DoctorId { get; set; }
    public Guid CheckedInByUserId { get; set; }
    public CheckInType CheckInType { get; set; }
    public CheckInDestination Destination { get; set; }
    public PrescribedBy PrescribedBy { get; set; }
    public SmsAlertOption SmsAlert { get; set; }
    public CheckInStatus Status { get; set; } = CheckInStatus.CheckedIn;
    public string? PackageName { get; set; }
    public DateTime CheckedInAtUtc { get; set; } = DateTime.UtcNow;
    public decimal SubTotal { get; set; }

    public Branch Branch { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public Doctor? Doctor { get; set; }
    public User CheckedInByUser { get; set; } = null!;
    public ICollection<CheckInServiceLine> ServiceLines { get; set; } = [];
    public Challan? Challan { get; set; }
    public Consultation? Consultation { get; set; }
}

public class CheckInServiceLine : BaseEntity
{
    public Guid CheckInId { get; set; }
    public Guid CatalogServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public decimal Charges { get; set; }
    public DateOnly? DeliveryDate { get; set; }

    public CheckIn CheckIn { get; set; } = null!;
    public CatalogService CatalogService { get; set; } = null!;
    public LabWorkItem? LabWorkItem { get; set; }
}

public class Challan : MigratableEntity
{
    public Guid CheckInId { get; set; }
    public Guid BranchId { get; set; }
    public string ChallanNumber { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal PatientPayable { get; set; }
    public decimal PanelPayable { get; set; }
    public PanelBillingMode? PanelBillingMode { get; set; }
    public Guid? CorporatePanelId { get; set; }
    public DateTime IssuedAtUtc { get; set; } = DateTime.UtcNow;
    public Guid IssuedByUserId { get; set; }

    public CheckIn CheckIn { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
    public User IssuedByUser { get; set; } = null!;
    public CorporatePanel? CorporatePanel { get; set; }
}

public class Consultation : MigratableEntity
{
    public Guid CheckInId { get; set; }
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public ConsultationStatus Status { get; set; } = ConsultationStatus.InProgress;
    public bool ReferToCategory { get; set; }
    public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAtUtc { get; set; }

    public CheckIn CheckIn { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public Doctor Doctor { get; set; } = null!;
    public ICollection<ConsultationEntry> Entries { get; set; } = [];
    public ICollection<ConsultationPrescription> Prescriptions { get; set; } = [];
}

public class ConsultationEntry : BaseEntity
{
    public Guid ConsultationId { get; set; }
    public ConsultationSectionType SectionType { get; set; }
    public string Content { get; set; } = string.Empty;
    public Guid? DoctorId { get; set; }
    public DateTime RecordedAtUtc { get; set; } = DateTime.UtcNow;

    public Consultation Consultation { get; set; } = null!;
    public Doctor? Doctor { get; set; }
}

public class ConsultationPrescription : BaseEntity
{
    public Guid ConsultationId { get; set; }
    public Guid? MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public string? Dosage { get; set; }
    public string? Route { get; set; }
    public string? Strength { get; set; }
    public string? Instructions { get; set; }

    public Consultation Consultation { get; set; } = null!;
    public Medicine? Medicine { get; set; }
}
