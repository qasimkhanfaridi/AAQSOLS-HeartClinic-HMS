using HeartClinicHms.Domain.Common;

namespace HeartClinicHms.Domain.Entities;

public class Branch : MigratableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Region { get; set; }
    public string? Phone { get; set; }
    public bool IsHeadOffice { get; set; }

    public ICollection<User> Users { get; set; } = [];
    public ICollection<Patient> Patients { get; set; } = [];
}

public class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<User> Users { get; set; } = [];
}

public class User : MigratableEntity
{
    public Guid BranchId { get; set; }
    public Guid RoleId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? AvatarInitial { get; set; }
    public DateTime? LastLoginAtUtc { get; set; }

    public Branch Branch { get; set; } = null!;
    public Role Role { get; set; } = null!;
}

public class Department : MigratableEntity
{
    public Guid BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }

    public Branch Branch { get; set; } = null!;
    public ICollection<Doctor> Doctors { get; set; } = [];
}

public class Doctor : MigratableEntity
{
    public Guid BranchId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Specialty { get; set; }
    public string? RegistrationNumber { get; set; }

    public Branch Branch { get; set; } = null!;
    public Department? Department { get; set; }
    public User? User { get; set; }
    public ICollection<CheckIn> CheckIns { get; set; } = [];
}
