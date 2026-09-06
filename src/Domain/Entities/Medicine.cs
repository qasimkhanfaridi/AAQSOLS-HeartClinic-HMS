using HeartClinicHms.Domain.Common;

namespace HeartClinicHms.Domain.Entities;

public class MedicineCategory : MigratableEntity
{
    public string Name { get; set; } = string.Empty;
    public ICollection<Medicine> Medicines { get; set; } = [];
}

public class MedicineGeneric : MigratableEntity
{
    public string Name { get; set; } = string.Empty;
    public ICollection<Medicine> Medicines { get; set; } = [];
}

public class MedicineDosageForm : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public ICollection<Medicine> Medicines { get; set; } = [];
}

public class MedicineRoute : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public ICollection<Medicine> Medicines { get; set; } = [];
}

public class Medicine : MigratableEntity
{
    public Guid? CategoryId { get; set; }
    public Guid? GenericId { get; set; }
    public Guid? DosageFormId { get; set; }
    public Guid? RouteId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? DosageSchedule { get; set; }
    public string? Strength { get; set; }
    public decimal StockQuantity { get; set; }

    public MedicineCategory? Category { get; set; }
    public MedicineGeneric? Generic { get; set; }
    public MedicineDosageForm? DosageForm { get; set; }
    public MedicineRoute? Route { get; set; }
}
