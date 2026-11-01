namespace Reminders.Application.Contracts;

public sealed record ReminderTaskInput(
    string Title,
    string Organization,
    ReminderScheduleInput Schedule,
    long? ChatId,
    DateOnly? ActiveFrom,
    bool IsEnabled);
