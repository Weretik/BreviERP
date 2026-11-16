namespace ActivityLog.Contracts.Activity;

/// <summary>The actions «Журнал дій» knows (ACT-001 R-02).</summary>
public static class ActivityActions
{
    public const string Created = "Created";
    public const string Changed = "Changed";
    public const string Deleted = "Deleted";
    public const string Sent = "Sent";
    public const string Closed = "Closed";
    public const string Cancelled = "Cancelled";
    public const string Reopened = "Reopened";
    public const string Voided = "Voided";
    public const string Restored = "Restored";
    public const string Imported = "Imported";

    public static IReadOnlyList<string> All { get; } =
        [Created, Changed, Deleted, Sent, Closed, Cancelled, Reopened, Voided, Restored, Imported];
}
