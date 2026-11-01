namespace Reminders.Domain.Occurrences;

/// <summary>Journal row: who pressed which button, or reopened the period in Admin, and when.</summary>
public sealed class ReminderAction
{
    private ReminderAction() { }

    internal ReminderAction(ReminderActionKind kind, ReminderActor actor, DateTimeOffset at, DateOnly? snoozedUntil)
    {
        Kind = kind;
        Actor = actor;
        At = at;
        SnoozedUntil = snoozedUntil;
    }

    public long Id { get; private set; }
    public ReminderActionKind Kind { get; private set; }
    public ReminderActor Actor { get; private set; } = null!;
    public DateTimeOffset At { get; private set; }
    public DateOnly? SnoozedUntil { get; private set; }
}
