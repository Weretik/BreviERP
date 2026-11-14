using Mediator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Scheduling.Application.Features.Runs.Commands.FinishScheduledTaskRun;
using Scheduling.Application.Features.Runs.Commands.StartScheduledTaskRun;

namespace Host.Seed.Jobs.Tasks;

/// <summary>
/// --tasks=key:runId,key:runId: one execution of the brevierp-tasks Cloud Run Job, started by the scheduler tick
/// (SCH-001 R-04, R-06). Each run is started, executed in its own DI scope and finished with its summary or main reason.
/// A run the tick no longer holds (expired or replaced) is skipped. Exit code 1 when any task failed.
/// </summary>
internal static class ScheduledTasksJobRunner
{
    public static async Task RunAsync(IServiceProvider services, IConfiguration configuration)
    {
        var runs = Parse(configuration["tasks"]);
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Tasks");
        logger.LogInformation("Tasks started: {Tasks}", string.Join(", ", runs.Select(run => run.Runner.Key)));

        var failed = new List<string>();
        foreach (var (runner, runId) in runs)
        {
            if (!await RunOneAsync(services, runner, runId))
                failed.Add(runner.Key);
        }

        if (failed.Count > 0)
            throw new InvalidOperationException($"Tasks failed: {string.Join(", ", failed)}.");
    }

    public static IReadOnlyList<(IScheduledTaskRunner Runner, Guid RunId)> Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException("--tasks needs key:runId pairs.");
        return value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(pair =>
            {
                var parts = pair.Split(':');
                if (parts.Length != 2 || !Guid.TryParse(parts[1], out var runId))
                    throw new InvalidOperationException($"Not a key:runId pair: {pair}.");
                var runner = ScheduledTaskRunners.Find(parts[0])
                    ?? throw new InvalidOperationException($"Unknown task: {parts[0]}.");
                return (runner, runId);
            })
            .ToArray();
    }

    private static async Task<bool> RunOneAsync(IServiceProvider services, IScheduledTaskRunner runner, Guid runId)
    {
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger($"Tasks.{runner.Key}");
        await using (var scope = services.CreateAsyncScope())
        {
            var started = await scope.ServiceProvider.GetRequiredService<ISender>()
                .Send(new StartScheduledTaskRunCommand(runner.Key, runId));
            if (!started.IsSuccess)
            {
                logger.LogWarning("{Task} skipped: run {RunId} is no longer current", runner.Key, runId);
                return true;
            }
        }

        bool succeeded;
        string message;
        try
        {
            await using var scope = services.CreateAsyncScope();
            message = await runner.RunAsync(scope.ServiceProvider, logger, CancellationToken.None);
            succeeded = true;
            logger.LogInformation("{Task} OK: {Summary}", runner.Key, message);
        }
        catch (Exception error)
        {
            message = error.Message;
            succeeded = false;
            logger.LogError("{Task} FAILED: {ErrorType}: {Reason}", runner.Key, error.GetType().Name, error.Message);
        }

        await using (var scope = services.CreateAsyncScope())
        {
            var finished = await scope.ServiceProvider.GetRequiredService<ISender>()
                .Send(new FinishScheduledTaskRunCommand(runner.Key, runId, succeeded, message));
            if (!finished.IsSuccess)
                logger.LogWarning("{Task}: result not recorded, run {RunId} is no longer current", runner.Key, runId);
        }

        return succeeded;
    }
}
