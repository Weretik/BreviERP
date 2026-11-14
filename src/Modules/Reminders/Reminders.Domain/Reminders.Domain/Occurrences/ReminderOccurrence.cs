using BuildingBlocks.Domain.Entity;
using Reminders.Domain.Identifiers;
using Reminders.Domain.Schedules;

namespace Reminders.Domain.Occurrences;

/// <summary>One period of a task, for example "ПДВ декларація за жовтень 2026". Its deadline never changes.</summary>
public sealed class ReminderOccurrence : BaseEntity<ReminderOccurrenceId>, IAggregateRoot
{
    private readonly List<ReminderAction> _actions = [];

    private ReminderOccurrence() { }

    public ReminderTaskId TaskId { get; private set; }
    public ReminderPeriodicity Periodicity { get; private set; }
    public DateOnly PeriodStart { get; private set; }
    public DateOnly DueDate { get; private set; }
    public ReminderOccurrenceStatus Status { get; private set; }
    public DateOnly? SnoozedUntil { get; private set; }
    public ReminderActor? ClosedBy { get; private set; }
    public DateTimeOffset? ClosedAt { get; private set; }
    public ReminderActor? PostponedBy { get; private set; }
    public DateTimeOffset? PostponedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public IReadOnlyCollection<ReminderAction> Actions => _actions.AsReadOnly();

    public ReminderPeriod Period => new(Periodicity, PeriodStart);

    public static ReminderOccurrence Open(ReminderTaskId taskId, ReminderPeriod period, DateOnly dueDate, DateTimeOffset now) => new()
    {
        TaskId = taskId,
        Periodicity = period.Periodicity,
        PeriodStart = period.Start,
        DueDate = dueDate,
        Status = ReminderOccurrenceStatus.Open,
        CreatedAt = now
    };

    /// <summary>After the task's rule changes, an open period follows the new deadline; closed periods keep history.</summary>
    public void Reschedule(DateOnly dueDate)
    {
        if (Status == ReminderOccurrenceStatus.Open) DueDate = dueDate;
    }

    public bool IsPostponedOn(DateOnly today) => Status == ReminderOccurrenceStatus.Open && SnoozedUntil > today;

    /// <returns>False when the occurrence was already closed; the first close wins.</returns>
    public bool Close(ReminderActor actor, DateTimeOffset now)
    {
        if (Status == ReminderOccurrenceStatus.Done) return false;
        Status = ReminderOccurrenceStatus.Done;
        ClosedBy = actor;
        ClosedAt = now;
        SnoozedUntil = null;
        _actions.Add(new ReminderAction(ReminderActionKind.Done, actor, now, null));
        return true;
    }

    public bool Postpone(ReminderActor actor, DateTimeOffset now, DateOnly until)
    {
        if (Status == ReminderOccurrenceStatus.Done) return false;
        SnoozedUntil = until;
        PostponedBy = actor;
        PostponedAt = now;
        _actions.Add(new ReminderAction(ReminderActionKind.Postponed, actor, now, until));
        return true;
    }

    public bool Reopen(ReminderActor actor, DateTimeOffset now)
    {
        if (Status == ReminderOccurrenceStatus.Open) return false;
        Status = ReminderOccurrenceStatus.Open;
        ClosedBy = null;
        ClosedAt = null;
        _actions.Add(new ReminderAction(ReminderActionKind.Reopened, actor, now, null));
        return true;
    }
}
