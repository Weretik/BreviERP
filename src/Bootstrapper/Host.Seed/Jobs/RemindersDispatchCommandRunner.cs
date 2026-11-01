using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Reminders.Application.Dispatch;

namespace Host.Seed.Jobs;

/// <summary>
/// --reminders-dispatch=true: the daily Telegram reminder run. Cloud Scheduler executes the
/// brevierp-seed Cloud Run Job with this argument on weekdays at 09:00 Kyiv. Only this mode runs:
/// no migrations or seeding. Safe to rerun the same day. Exit code 1 when any card failed.
/// </summary>
internal static class RemindersDispatchCommandRunner
{
    public static async Task RunAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("RemindersDispatch");
        var summary = await scope.ServiceProvider.GetRequiredService<ReminderDispatcher>().RunAsync(CancellationToken.None);
        logger.LogInformation("Reminders {Date}: working day {WorkingDay}, new periods {Created}, sent {Sent}, failed {Failed}",
            summary.Date, summary.WorkingDay, summary.CreatedOccurrences, summary.Sent, summary.Failed);
        if (summary.Failed > 0) throw new InvalidOperationException($"{summary.Failed} reminder(s) were not sent.");
    }
}
