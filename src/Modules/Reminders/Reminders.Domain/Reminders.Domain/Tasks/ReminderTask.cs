using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Entity;
using BuildingBlocks.Domain.Results;
using Reminders.Domain.Identifiers;

namespace Reminders.Domain.Tasks;

/// <summary>A recurring obligation configured in Admin. Each period becomes one occurrence.</summary>
public sealed class ReminderTask : BaseAuditableEntity<ReminderTaskId>, IAggregateRoot
{
    public const int TitleMaxLength = 200;

    private ReminderTask() { }

    public string Title { get; private set; } = string.Empty;
    public ReminderOrganization Organization { get; private set; }
    public ReminderSchedule Schedule { get; private set; } = null!;
    public ReminderChatId? ChatId { get; private set; }

    /// <summary>Only periods whose deadline is on or after this date are reminded.</summary>
    public DateOnly ActiveFrom { get; private set; }
    public bool IsEnabled { get; private set; }

    public static DomainResult<ReminderTask> Create(string title, ReminderOrganization organization, ReminderSchedule schedule,
        ReminderChatId? chatId, DateOnly activeFrom, bool isEnabled, DateTimeOffset now)
    {
        var task = new ReminderTask();
        var applied = task.Apply(title, organization, schedule, chatId, activeFrom, isEnabled);
        if (applied.IsFailure) return applied.Error!;
        task.MarkAsCreated(now);
        return task;
    }

    public DomainResult Revise(string title, ReminderOrganization organization, ReminderSchedule schedule,
        ReminderChatId? chatId, DateOnly activeFrom, bool isEnabled, DateTimeOffset now)
    {
        var applied = Apply(title, organization, schedule, chatId, activeFrom, isEnabled);
        if (applied.IsSuccess) MarkAsUpdated(now);
        return applied;
    }

    /// <summary>Points the task at another chat; nothing else changes, so no rule can fail.</summary>
    public void AssignChat(ReminderChatId chatId, DateTimeOffset now)
    {
        ChatId = chatId;
        MarkAsUpdated(now);
    }

    public bool CanSend => IsEnabled && !IsDeleted && ChatId is not null;

    private DomainResult Apply(string title, ReminderOrganization organization, ReminderSchedule schedule,
        ReminderChatId? chatId, DateOnly activeFrom, bool isEnabled)
    {
        ArgumentNullException.ThrowIfNull(schedule);
        var trimmed = title?.Trim() ?? string.Empty;
        if (trimmed.Length is 0 or > TitleMaxLength) return ReminderTaskErrors.TitleInvalid();
        if (schedule.LeadDays is < 0 or > ReminderSchedule.MaxLeadDays) return ReminderTaskErrors.LeadDaysOutOfRange();
        Title = trimmed;
        Organization = organization;
        Schedule = schedule;
        ChatId = chatId;
        ActiveFrom = activeFrom;
        IsEnabled = isEnabled;
        return DomainResult.Success();
    }
}
