using HeartClinicHms.Domain.Common;

namespace HeartClinicHms.Domain.Entities;

public class CorporatePanel : MigratableEntity
{
    public Guid BranchId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }

    public Branch Branch { get; set; } = null!;
    public ICollection<PanelServiceRate> ServiceRates { get; set; } = [];
    public ICollection<Patient> Patients { get; set; } = [];
}

public class PanelServiceRate : BaseEntity
{
    public Guid CorporatePanelId { get; set; }
    public Guid CatalogServiceId { get; set; }
    public decimal PanelRate { get; set; }

    public CorporatePanel CorporatePanel { get; set; } = null!;
    public CatalogService CatalogService { get; set; } = null!;
}
