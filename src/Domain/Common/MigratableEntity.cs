namespace HeartClinicHms.Domain.Common;

/// <summary>
/// Base for entities that may be imported from LOOM or other legacy HMIS systems.
/// LegacyExternalId stores the original primary key from the source system.
/// </summary>
public abstract class MigratableEntity : BaseEntity
{
    public string? LegacySourceSystem { get; set; }
    public string? LegacyExternalId { get; set; }
    public Guid? ImportBatchId { get; set; }
    public DateTime? ImportedAtUtc { get; set; }
    public bool IsLegacyRecord { get; set; }
}
