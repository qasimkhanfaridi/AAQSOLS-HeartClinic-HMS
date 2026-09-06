namespace HeartClinicHms.Domain.Enums;

public enum PatientGender
{
    Male = 1,
    Female = 2,
    Other = 3
}

public enum CheckInType
{
    WalkIn = 1,
    Referral = 2
}

public enum CheckInDestination
{
    Department = 1,
    Doctor = 2,
    InvestigationsDiagnostics = 3
}

public enum PrescribedBy
{
    Doctor = 1,
    Self = 2,
    Outdoor = 3
}

public enum SmsAlertOption
{
    Patient = 1,
    Organization = 2,
    Both = 3,
    None = 4
}

public enum CheckInStatus
{
    CheckedIn = 1,
    InConsultation = 2,
    OnHold = 3,
    Referred = 4,
    Completed = 5,
    Cancelled = 6
}

public enum ConsultationStatus
{
    InProgress = 1,
    OnHold = 2,
    Referred = 3,
    Completed = 4
}

public enum ConsultationSectionType
{
    OtherComplaints = 1,
    ExamFindings = 2,
    PrimaryDiagnosis = 3,
    SecondaryDiagnosis = 4,
    Diagnostics = 5,
    Investigations = 6,
    Medicines = 7,
    Instructions = 8,
    FollowUps = 9,
    Advice = 10
}

public enum ImportBatchStatus
{
    Pending = 1,
    Running = 2,
    Completed = 3,
    Failed = 4,
    PartiallyCompleted = 5
}

public enum PanelBillingMode
{
    FullPanel = 1,
    SplitHalf = 2,
    PanelClaim = 3
}

public enum LabWorkStatus
{
    PendingPayment = 1,
    PaymentVerified = 2,
    SampleCollected = 3,
    Completed = 4
}
