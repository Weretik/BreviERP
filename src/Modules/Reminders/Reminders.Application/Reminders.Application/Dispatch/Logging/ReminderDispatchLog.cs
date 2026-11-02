namespace Reminders.Application.Dispatch.Logging;

/// <summary>Daily run log: one line per decision, so Cloud Run logs show what was found, sent or skipped and why.</summary>
public static partial class ReminderDispatchLog
{
    [LoggerMessage(EventId = 2101, Level = LogLevel.Information,
        Message = "Reminders {Date}: non-working day, nothing is sent")]
    public static partial void NonWorkingDay(ILogger logger, DateOnly date);

    [LoggerMessage(EventId = 2102, Level = LogLevel.Information,
        Message = "Reminders {Date}: {Active} active task(s) with a chat, {WithoutChat} active task(s) without a chat")]
    public static partial void TasksFound(ILogger logger, DateOnly date, int active, int withoutChat);

    [LoggerMessage(EventId = 2103, Level = LogLevel.Warning,
        Message = "Task «{Task}» has no chat assigned and is skipped")]
    public static partial void TaskWithoutChat(ILogger logger, string task);

    [LoggerMessage(EventId = 2104, Level = LogLevel.Information,
        Message = "Task «{Task}» due {DueDate}: reminder window not open yet")]
    public static partial void NotDueYet(ILogger logger, string task, DateOnly dueDate);

    [LoggerMessage(EventId = 2110, Level = LogLevel.Information,
        Message = "Task «{Task}»: no open period in the reminder window")]
    public static partial void NothingDue(ILogger logger, string task);

    [LoggerMessage(EventId = 2105, Level = LogLevel.Information,
        Message = "Task «{Task}» due {DueDate}: already sent today")]
    public static partial void AlreadySent(ILogger logger, string task, DateOnly dueDate);

    [LoggerMessage(EventId = 2106, Level = LogLevel.Warning,
        Message = "Task «{Task}» due {DueDate}: chat is not approved or the bot left it, skipped")]
    public static partial void ChatUnavailable(ILogger logger, string task, DateOnly dueDate);

    [LoggerMessage(EventId = 2107, Level = LogLevel.Information,
        Message = "Task «{Task}» due {DueDate}: sent to «{Chat}»")]
    public static partial void Sent(ILogger logger, string task, DateOnly dueDate, string chat);

    [LoggerMessage(EventId = 2108, Level = LogLevel.Error,
        Message = "Task «{Task}» due {DueDate}: not sent to «{Chat}», {ErrorType}: {Reason}")]
    public static partial void SendFailed(ILogger logger, string task, DateOnly dueDate, string chat, string errorType, string reason);

    [LoggerMessage(EventId = 2109, Level = LogLevel.Warning,
        Message = "Task «{Task}»: old buttons were not removed, {ErrorType}")]
    public static partial void OldButtonsKept(ILogger logger, string task, string errorType);
}
