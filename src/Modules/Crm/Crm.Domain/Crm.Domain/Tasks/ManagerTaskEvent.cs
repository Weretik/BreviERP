namespace Crm.Domain.Tasks;

/// <summary>A row of «Історія змін»: who changed what and when (R-11).</summary>
public sealed class ManagerTaskEvent
{
    private ManagerTaskEvent() { }

    internal ManagerTaskEvent(ManagerTaskEventKind kind, ManagerTaskPerson by, DateTimeOffset at, string? details)
    {
        Kind = kind;
        ByUserId = by.UserId;
        ByName = by.Name;
        At = at;
        Details = details;
    }

    public long Id { get; private set; }
    public long TaskId { get; private set; }
    public ManagerTaskEventKind Kind { get; private set; }
    public int ByUserId { get; private set; }
    public string ByName { get; private set; } = null!;
    public DateTimeOffset At { get; private set; }

    /// <summary>Kind-specific value: new status, new assignee name, new due time (ISO 8601), step text.</summary>
    public string? Details { get; private set; }
}
