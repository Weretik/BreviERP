using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Reminders.Application.Features.Dispatch.Shared;

namespace Host.Seed.Jobs;

/// <summary>
/// --reminders-dispatch=true: the daily Telegram reminder run. Cloud Scheduler executes the
/// brevierp-reminders Cloud Run Job (same image, this argument baked in) on weekdays at 09:00 Kyiv.
/// Only this mode runs: no migrations or seeding. Safe to rerun the same day. Exit code 1 when the run
/// failed or any card was not sent.
/// </summary>
internal static class RemindersDispatchCommandRunner
{
    public static async Task RunAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("RemindersDispatch");
        logger.LogInformation("Reminders run started");
        ReminderDispatchSummary summary;
        try
        {
            summary = await scope.ServiceProvider.GetRequiredService<ReminderDispatcher>().RunAsync(CancellationToken.None);
        }
        catch (Exception error)
        {
            logger.LogError("Reminders run FAILED: {ErrorType}: {Reason}", error.GetType().Name, error.Message);
            throw;
        }

        var result = summary.Failed > 0 ? "FAILED" : "OK";
        logger.LogInformation(
            "Reminders {Date}: {Result}. Working day {WorkingDay}; active tasks {Active}, without chat {WithoutChat}; " +
            "new periods {Created}; sent {Sent}, not due yet {NotDue}, already sent today {AlreadySent}, " +
            "chat unavailable {ChatUnavailable}, failed {Failed}",
            summary.Date, result, summary.WorkingDay, summary.ActiveTasks, summary.TasksWithoutChat,
            summary.CreatedOccurrences, summary.Sent, summary.NotDueYet, summary.AlreadySent,
            summary.ChatUnavailable, summary.Failed);
        if (summary.Failed > 0) throw new InvalidOperationException($"{summary.Failed} reminder(s) were not sent.");
    }
}
