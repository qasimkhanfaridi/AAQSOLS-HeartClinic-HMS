using HeartClinicHms.Domain.Common;
using HeartClinicHms.Domain.Enums;

namespace HeartClinicHms.Domain.Entities;

public class PatientType : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public ICollection<Patient> Patients { get; set; } = [];
}

public class Patient : MigratableEntity
{
    public Guid BranchId { get; set; }
    public Guid PatientTypeId { get; set; }
    public Guid? CorporatePanelId { get; set; }
    public string MrNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string GuardianName { get; set; } = string.Empty;
    public PatientGender Gender { get; set; }
    public string CnicOrPassport { get; set; } = string.Empty;
    public string? Mobile { get; set; }
    public string? Address { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? LegacyMrNumber { get; set; }

    public Branch Branch { get; set; } = null!;
    public PatientType PatientType { get; set; } = null!;
    public CorporatePanel? CorporatePanel { get; set; }
    public ICollection<CheckIn> CheckIns { get; set; } = [];
}

public class MrNumberSequence : BaseEntity
{
    public Guid BranchId { get; set; }
    public int Year { get; set; }
    public int Prefix { get; set; } = 5601;
    public int LastSequence { get; set; }

    public Branch Branch { get; set; } = null!;
}
