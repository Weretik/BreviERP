using Microsoft.Extensions.Logging;
using Reminders.Application.Features.Dispatch.Commands.DispatchReminders;

namespace Host.Seed.Jobs.Tasks.Office;

/// <summary>
/// Telegram reminders of «Бухгалтерія»: the daily run that sends due cards. Safe to rerun the same day; fails when any
/// card was not sent.
/// </summary>
internal sealed class RemindersTaskRunner : IScheduledTaskRunner
{
    public string Key => "reminders";

    public async Task<string> RunAsync(IServiceProvider services, ILogger logger, CancellationToken cancellationToken)
    {
        var summary = await services.SendAsync(new DispatchRemindersCommand(), cancellationToken);
        logger.LogInformation(
            "Reminders {Date}: working day {WorkingDay}; active tasks {Active}, without chat {WithoutChat}; " +
            "new periods {Created}; sent {Sent}, not due yet {NotDue}, already sent today {AlreadySent}, " +
            "chat unavailable {ChatUnavailable}, failed {Failed}",
            summary.Date, summary.WorkingDay, summary.ActiveTasks, summary.TasksWithoutChat,
            summary.CreatedOccurrences, summary.Sent, summary.NotDueYet, summary.AlreadySent,
            summary.ChatUnavailable, summary.Failed);
        if (summary.Failed > 0)
            throw new InvalidOperationException($"Не надіслано нагадувань: {summary.Failed}.");
        return summary.WorkingDay
            ? $"Надіслано {summary.Sent}, уже надіслано сьогодні {summary.AlreadySent}, чат недоступний {summary.ChatUnavailable}."
            : "Неробочий день: нічого не надсилали.";
    }
}
