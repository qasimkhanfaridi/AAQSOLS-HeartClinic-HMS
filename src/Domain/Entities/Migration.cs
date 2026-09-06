using HeartClinicHms.Domain.Common;
using HeartClinicHms.Domain.Enums;

namespace HeartClinicHms.Domain.Entities;

public class ImportBatch : BaseEntity
{
    public string SourceSystem { get; set; } = "LOOM";
    public string EntityType { get; set; } = string.Empty;
    public ImportBatchStatus Status { get; set; } = ImportBatchStatus.Pending;
    public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAtUtc { get; set; }
    public int TotalRecords { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public string? Notes { get; set; }

    public ICollection<ImportMappingLog> MappingLogs { get; set; } = [];
}

public class ImportMappingLog : BaseEntity
{
    public Guid ImportBatchId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string LegacyExternalId { get; set; } = string.Empty;
    public Guid? NewEntityId { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? RawPayloadJson { get; set; }

    public ImportBatch ImportBatch { get; set; } = null!;
}

public class AuditLog : BaseEntity
{
    public Guid? UserId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? DetailsJson { get; set; }
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}
