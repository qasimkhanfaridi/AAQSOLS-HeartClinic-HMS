using HeartClinicHms.Domain.Common;
using HeartClinicHms.Domain.Enums;

namespace HeartClinicHms.Domain.Entities;

public class LabWorkItem : BaseEntity
{
    public Guid CheckInServiceLineId { get; set; }
    public LabWorkStatus Status { get; set; } = LabWorkStatus.PendingPayment;
    public DateTime? PaymentVerifiedAtUtc { get; set; }
    public DateTime? SampleCollectedAtUtc { get; set; }
    public Guid? PaymentVerifiedByUserId { get; set; }
    public Guid? SampleCollectedByUserId { get; set; }

    public CheckInServiceLine CheckInServiceLine { get; set; } = null!;
    public User? PaymentVerifiedByUser { get; set; }
    public User? SampleCollectedByUser { get; set; }
}
