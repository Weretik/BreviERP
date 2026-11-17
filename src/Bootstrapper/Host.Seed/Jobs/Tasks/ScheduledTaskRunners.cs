using Host.Seed.Jobs.Tasks.Keepin;
using Host.Seed.Jobs.Tasks.Maintenance;
using Host.Seed.Jobs.Tasks.Office;
using Host.Seed.Jobs.Tasks.Pricing;

namespace Host.Seed.Jobs.Tasks;

/// <summary>
/// The runner of every catalogue task that runs in the job (all except the outbox, which runs inside the tick).
/// <see cref="Sync"/> keeps the order of the old brevierp-sync job (SCH-001 R-12).
/// </summary>
internal static class ScheduledTaskRunners
{
    public static IReadOnlyList<IScheduledTaskRunner> Sync { get; } =
    [
        new KeepinSuppliersTaskRunner(),
        new KeepinClientsTaskRunner(),
        new SewingPricesTaskRunner(),
        new MediaThumbnailsTaskRunner(),
        new PpeSupplierPricesTaskRunner(),
        new PurchaseInvoicesTaskRunner(),
        new LeadsTaskRunner(),
    ];

    public static IReadOnlyList<IScheduledTaskRunner> All { get; } =
        [.. Sync, new RemindersTaskRunner(), new ActivityLogCleanupTaskRunner(), new PurchaseRecognitionTaskRunner()];

    public static IScheduledTaskRunner? Find(string key) => All.FirstOrDefault(runner => runner.Key == key);
}
