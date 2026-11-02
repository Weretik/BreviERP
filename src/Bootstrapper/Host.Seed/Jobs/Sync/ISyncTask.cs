using Microsoft.Extensions.Logging;

namespace Host.Seed.Jobs.Sync;

/// <summary>
/// One step of the shared daily sync job (brevierp-sync): a refresh from an external system or an import.
/// A step must be safe to repeat and must leave data unchanged when it fails.
/// </summary>
internal interface ISyncTask
{
    /// <summary>Stable kebab-case name used in logs and in --sync-tasks.</summary>
    string Name { get; }

    Task RunAsync(IServiceProvider services, ILogger logger, CancellationToken cancellationToken);
}
