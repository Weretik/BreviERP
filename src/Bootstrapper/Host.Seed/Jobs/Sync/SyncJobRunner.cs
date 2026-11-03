using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Host.Seed.Jobs.Sync;

/// <summary>
/// --sync=true: the shared daily job brevierp-sync (Cloud Scheduler). Runs every registered step in order,
/// each in its own DI scope. A failed step is logged and the next one still runs; the job exits with code 1
/// when any step failed. No migrations or seeding. --sync-tasks=a,b runs only the named steps.
/// To add a step: implement <see cref="ISyncTask"/>, add it to <see cref="Tasks"/>, register its services in
/// SeedHostServicesExtensions and attach its secrets to the job in the deploy workflow.
/// </summary>
internal static class SyncJobRunner
{
    private static readonly ISyncTask[] Tasks =
    [
        new KeepinSuppliersSyncTask(),
        new KeepinClientsSyncTask(),
        new SewingPricesSyncTask(),
    ];

    public static async Task RunAsync(IServiceProvider services, IConfiguration configuration)
    {
        var selected = Select(configuration["sync-tasks"]);
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Sync");
        logger.LogInformation("Sync started: {Tasks}", string.Join(", ", selected.Select(task => task.Name)));

        var failed = new List<string>();
        foreach (var task in selected)
        {
            var taskLogger = services.GetRequiredService<ILoggerFactory>().CreateLogger($"Sync.{task.Name}");
            taskLogger.LogInformation("{Task} started", task.Name);
            try
            {
                await using var scope = services.CreateAsyncScope();
                await task.RunAsync(scope.ServiceProvider, taskLogger, CancellationToken.None);
                taskLogger.LogInformation("{Task} OK", task.Name);
            }
            catch (Exception error)
            {
                failed.Add(task.Name);
                taskLogger.LogError("{Task} FAILED: {ErrorType}: {Reason}", task.Name, error.GetType().Name, error.Message);
            }
        }

        if (failed.Count > 0)
            throw new InvalidOperationException($"Sync failed: {string.Join(", ", failed)}.");
        logger.LogInformation("Sync finished: all {Count} steps OK", selected.Count);
    }

    private static IReadOnlyList<ISyncTask> Select(string? names)
    {
        if (string.IsNullOrWhiteSpace(names))
            return Tasks;

        var requested = names.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var unknown = requested.Where(name => Tasks.All(task => task.Name != name)).ToArray();
        if (unknown.Length > 0)
            throw new InvalidOperationException(
                $"Unknown sync task: {string.Join(", ", unknown)}. Known: {string.Join(", ", Tasks.Select(task => task.Name))}.");
        return Tasks.Where(task => requested.Contains(task.Name)).ToArray();
    }
}
