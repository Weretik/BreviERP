using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Host.Seed.Jobs.Tasks.Office;

namespace Host.Seed.Jobs;

/// <summary>
/// --reminders-dispatch=true: the old daily Telegram reminder job brevierp-reminders, kept until the switch-over to
/// «Фонові задачі» (SCH-001 R-12). Runs the same code as the «reminders» task. Only this mode runs: no migrations or
/// seeding. Exit code 1 when the run failed or any card was not sent.
/// </summary>
internal static class RemindersDispatchCommandRunner
{
    public static async Task RunAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("RemindersDispatch");
        logger.LogInformation("Reminders run started");
        try
        {
            var summary = await new RemindersTaskRunner().RunAsync(scope.ServiceProvider, logger, CancellationToken.None);
            logger.LogInformation("Reminders run OK: {Summary}", summary);
        }
        catch (Exception error)
        {
            logger.LogError("Reminders run FAILED: {ErrorType}: {Reason}", error.GetType().Name, error.Message);
            throw;
        }
    }
}
