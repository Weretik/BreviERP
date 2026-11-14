using Microsoft.Extensions.Logging;

namespace Host.Seed.Jobs.Tasks;

/// <summary>
/// The code of one background task of «Фонові задачі» (SCH-001): a refresh from an external system, an import or a
/// dispatch. A task must be safe to repeat and must leave data unchanged when it fails. It returns a short Ukrainian
/// summary for the admin page and throws with the main reason when it fails.
/// </summary>
internal interface IScheduledTaskRunner
{
    /// <summary>The task key of ScheduledTaskCatalog; also the name in logs and in --sync-tasks.</summary>
    string Key { get; }

    Task<string> RunAsync(IServiceProvider services, ILogger logger, CancellationToken cancellationToken);
}
