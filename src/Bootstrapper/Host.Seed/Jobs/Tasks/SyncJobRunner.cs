using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Host.Seed.Jobs.Tasks;

/// <summary>
/// --sync=true: the old shared daily job brevierp-sync, kept until the switch-over to «Фонові задачі» (SCH-001 R-12).
/// Runs the sync tasks in order, each in its own DI scope. A failed step is logged and the next one still runs; the job
/// exits with code 1 when any step failed. No migrations or seeding. --sync-tasks=a,b runs only the named steps.
/// </summary>
internal static class SyncJobRunner
{
    public static async Task RunAsync(IServiceProvider services, IConfiguration configuration)
    {
        var selected = Select(configuration["sync-tasks"]);
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Sync");
        logger.LogInformation("Sync started: {Tasks}", string.Join(", ", selected.Select(task => task.Key)));

        var failed = new List<string>();
        foreach (var task in selected)
        {
            var taskLogger = services.GetRequiredService<ILoggerFactory>().CreateLogger($"Sync.{task.Key}");
            taskLogger.LogInformation("{Task} started", task.Key);
            try
            {
                await using var scope = services.CreateAsyncScope();
                var summary = await task.RunAsync(scope.ServiceProvider, taskLogger, CancellationToken.None);
                taskLogger.LogInformation("{Task} OK: {Summary}", task.Key, summary);
            }
            catch (Exception error)
            {
                failed.Add(task.Key);
                taskLogger.LogError("{Task} FAILED: {ErrorType}: {Reason}", task.Key, error.GetType().Name, error.Message);
            }
        }

        if (failed.Count > 0)
            throw new InvalidOperationException($"Sync failed: {string.Join(", ", failed)}.");
        logger.LogInformation("Sync finished: all {Count} steps OK", selected.Count);
    }

    private static IReadOnlyList<IScheduledTaskRunner> Select(string? names)
    {
        var tasks = ScheduledTaskRunners.Sync;
        if (string.IsNullOrWhiteSpace(names))
            return tasks;

        var requested = names.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var unknown = requested.Where(name => tasks.All(task => task.Key != name)).ToArray();
        if (unknown.Length > 0)
            throw new InvalidOperationException(
                $"Unknown sync task: {string.Join(", ", unknown)}. Known: {string.Join(", ", tasks.Select(task => task.Key))}.");
        return tasks.Where(task => requested.Contains(task.Key)).ToArray();
    }
}
