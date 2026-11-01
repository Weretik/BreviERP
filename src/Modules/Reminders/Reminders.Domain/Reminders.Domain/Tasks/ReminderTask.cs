using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Entity;
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

    public static ReminderTask Create(string title, ReminderOrganization organization, ReminderSchedule schedule,
        ReminderChatId? chatId, DateOnly activeFrom, bool isEnabled, DateTimeOffset now)
    {
        var task = new ReminderTask();
        task.Apply(title, organization, schedule, chatId, activeFrom, isEnabled);
        task.MarkAsCreated(now);
        return task;
    }

    public void Revise(string title, ReminderOrganization organization, ReminderSchedule schedule,
        ReminderChatId? chatId, DateOnly activeFrom, bool isEnabled, DateTimeOffset now)
    {
        Apply(title, organization, schedule, chatId, activeFrom, isEnabled);
        MarkAsUpdated(now);
    }

    public bool CanSend => IsEnabled && !IsDeleted && ChatId is not null;

    private void Apply(string title, ReminderOrganization organization, ReminderSchedule schedule,
        ReminderChatId? chatId, DateOnly activeFrom, bool isEnabled)
    {
        var trimmed = title.Trim();
        if (trimmed.Length is 0 or > TitleMaxLength) throw new ArgumentException("Title is required.", nameof(title));
        if (schedule.LeadDays is < 0 or > ReminderSchedule.MaxLeadDays) throw new ArgumentOutOfRangeException(nameof(schedule));
        Title = trimmed;
        Organization = organization;
        Schedule = schedule;
        ChatId = chatId;
        ActiveFrom = activeFrom;
        IsEnabled = isEnabled;
    }
}
