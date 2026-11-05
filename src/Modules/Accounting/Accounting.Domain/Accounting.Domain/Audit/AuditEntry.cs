namespace Accounting.Domain.Audit;

/// <summary>
/// One entry of the single finance change log: who did what to which record, with JSON snapshots before and after.
/// Append-only; it survives the deletion of its record.
/// </summary>
public sealed class AuditEntry
{
    public const int ByMaxLength = 256;

    private AuditEntry() { }

    public AuditEntry(AuditEntity entity, Guid entityId, AuditAction action, string by, DateTimeOffset at,
        string? beforeJson, string? afterJson)
    {
        Entity = entity;
        EntityId = entityId;
        Action = action;
        By = by;
        At = at;
        BeforeJson = beforeJson;
        AfterJson = afterJson;
    }

    public long Id { get; private set; }
    public AuditEntity Entity { get; private set; }
    public Guid EntityId { get; private set; }
    public AuditAction Action { get; private set; }

    /// <summary>E-mail of the staff account.</summary>
    public string By { get; private set; } = null!;

    public DateTimeOffset At { get; private set; }
    public string? BeforeJson { get; private set; }
    public string? AfterJson { get; private set; }
}
