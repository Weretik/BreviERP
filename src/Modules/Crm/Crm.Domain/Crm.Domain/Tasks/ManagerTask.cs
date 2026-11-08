using System.Globalization;

namespace Crm.Domain.Tasks;

/// <summary>
/// A task that Director or Admin gives to a manager (TSK-001). The manager moves it through
/// <see cref="ManagerTaskStatus"/> and ticks its steps; everything else only Director and Admin change.
/// <see cref="Version"/> grows with every change and guards against stale saves (R-12).
/// </summary>
public sealed class ManagerTask
{
    private readonly List<ManagerTaskStep> _steps = [];
    private readonly List<ManagerTaskEvent> _events = [];

    private ManagerTask() { }

    public ManagerTask(ManagerTaskDraft draft, ManagerTaskPerson author, DateTimeOffset now)
    {
        AuthorUserId = author.UserId;
        AuthorName = author.Name;
        Status = ManagerTaskStatus.New;
        CreatedAt = now;
        UpdatedAt = now;
        Version = 1;
        Apply(draft);
        _events.Add(new ManagerTaskEvent(ManagerTaskEventKind.Created, author, now, null));
    }

    public long Id { get; private set; }
    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public int AssigneeUserId { get; private set; }
    public string AssigneeName { get; private set; } = null!;
    public int AuthorUserId { get; private set; }
    public string AuthorName { get; private set; } = null!;
    public DateTimeOffset DueAt { get; private set; }
    public bool IsUrgent { get; private set; }
    public ManagerTaskStatus Status { get; private set; }
    public Guid? CustomerPublicId { get; private set; }
    public string? CustomerName { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    /// <summary>The lead whose next call this task is (LEAD-001 R-08); null for tasks the director gives.</summary>
    public long? LeadId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public int Version { get; private set; }

    public IReadOnlyList<ManagerTaskStep> Steps => _steps;
    public IReadOnlyList<ManagerTaskEvent> Events => _events;

    /// <summary>«ЗД-0142»: the id with the prefix, never stored.</summary>
    public static string NumberOf(long id) => "ЗД-" + id.ToString("D4", CultureInfo.InvariantCulture);

    public static ManagerTaskLane LaneOf(ManagerTaskStatus status, DateTimeOffset dueAt, bool isUrgent,
        DateTimeOffset now) =>
        status == ManagerTaskStatus.Done ? ManagerTaskLane.Done
        : dueAt < now ? ManagerTaskLane.Overdue
        : isUrgent ? ManagerTaskLane.Urgent
        : ManagerTaskLane.Active;

    public ManagerTaskLane LaneAt(DateTimeOffset now) => LaneOf(Status, DueAt, IsUrgent, now);

    /// <summary>A «Подзвонити» task made by a recorded lead call (LEAD-001 R-08).</summary>
    public static ManagerTask ForLeadCall(long leadId, ManagerTaskDraft draft, ManagerTaskPerson author,
        DateTimeOffset now) => new(draft, author, now) { LeadId = leadId };

    /// <summary>Director or Admin saves the side panel; history records reassignment and a new due time.</summary>
    public void Edit(ManagerTaskDraft draft, ManagerTaskPerson by, DateTimeOffset now)
    {
        var reassigned = draft.Assignee.UserId != AssigneeUserId;
        var dueChanged = draft.DueAt != DueAt;
        var urgentChanged = draft.IsUrgent != IsUrgent;
        Apply(draft);

        _events.Add(new ManagerTaskEvent(ManagerTaskEventKind.Edited, by, now, null));
        if (reassigned) _events.Add(new ManagerTaskEvent(ManagerTaskEventKind.Reassigned, by, now, AssigneeName));
        if (dueChanged)
            _events.Add(new ManagerTaskEvent(ManagerTaskEventKind.DueChanged, by, now,
                DueAt.ToString("O", CultureInfo.InvariantCulture)));
        if (urgentChanged) _events.Add(UrgentEvent(by, now));
        Touch(now);
    }

    /// <summary>Returns false when the status is already the same (nothing changes, R-07).</summary>
    public bool ChangeStatus(ManagerTaskStatus status, ManagerTaskPerson by, DateTimeOffset now)
    {
        if (status == Status) return false;
        Status = status;
        CompletedAt = status == ManagerTaskStatus.Done ? now : null;
        _events.Add(new ManagerTaskEvent(ManagerTaskEventKind.StatusChanged, by, now, status.ToString()));
        Touch(now);
        return true;
    }

    public bool SetUrgent(bool isUrgent, ManagerTaskPerson by, DateTimeOffset now)
    {
        if (isUrgent == IsUrgent) return false;
        IsUrgent = isUrgent;
        _events.Add(UrgentEvent(by, now));
        Touch(now);
        return true;
    }

    /// <summary>Returns false when the step is not on this task.</summary>
    public bool MarkStep(long stepId, bool isDone, ManagerTaskPerson by, DateTimeOffset now)
    {
        var step = _steps.SingleOrDefault(x => x.Id == stepId);
        if (step is null) return false;
        if (step.IsDone == isDone) return true;
        step.Mark(isDone);
        _events.Add(new ManagerTaskEvent(
            isDone ? ManagerTaskEventKind.StepChecked : ManagerTaskEventKind.StepUnchecked, by, now, step.Text));
        Touch(now);
        return true;
    }

    private ManagerTaskEvent UrgentEvent(ManagerTaskPerson by, DateTimeOffset now) =>
        new(ManagerTaskEventKind.UrgentChanged, by, now, IsUrgent ? "true" : "false");

    private void Apply(ManagerTaskDraft draft)
    {
        Title = draft.Title.Trim();
        Description = string.IsNullOrWhiteSpace(draft.Description) ? null : draft.Description.Trim();
        AssigneeUserId = draft.Assignee.UserId;
        AssigneeName = draft.Assignee.Name;
        DueAt = draft.DueAt.ToUniversalTime();
        IsUrgent = draft.IsUrgent;
        CustomerPublicId = draft.CustomerPublicId;
        CustomerName = draft.CustomerPublicId is null ? null : draft.CustomerName;
        _steps.Clear();
        _steps.AddRange(draft.Steps.Select((x, i) => new ManagerTaskStep(i, x.Text.Trim(), x.IsDone)));
    }

    private void Touch(DateTimeOffset now)
    {
        UpdatedAt = now;
        Version++;
    }
}
