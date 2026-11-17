using ActivityLog.Application.Features.Entries.Commands.DeleteOldActivityEntries;
using Microsoft.Extensions.Logging;

namespace Host.Seed.Jobs.Tasks.Maintenance;

/// <summary>
/// «Журнал дій» keeps one year (ACT-001 R-12): deletes older entries every night. Safe to repeat; a repeat finds
/// nothing.
/// </summary>
internal sealed class ActivityLogCleanupTaskRunner : IScheduledTaskRunner
{
    public string Key => "activity-log-cleanup";

    public async Task<string> RunAsync(IServiceProvider services, ILogger logger, CancellationToken cancellationToken)
    {
        var deleted = await services.SendAsync(new DeleteOldActivityEntriesCommand(), cancellationToken);
        logger.LogInformation("Deleted {Deleted} activity log entries older than {Years} year(s)", deleted,
            DeleteOldActivityEntriesHandler.KeepYears);
        return deleted == 0 ? "Старих записів немає." : $"Видалено записів, старших за рік: {deleted}.";
    }
}
