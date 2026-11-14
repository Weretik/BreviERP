using Microsoft.Extensions.Logging;
using Crm.Application.Features.Customers.Commands.SyncKeepinCrmClients;

namespace Host.Seed.Jobs.Sync;

/// <summary>
/// KeepinCRM → Brevi client refresh, the same use case as «Оновити з KeepinCRM» on the clients tab. The daily run never
/// deletes: Brevi clients without a KeepinCRM link are left as they are (removeUnlinked is only for the one-time transfer).
/// </summary>
internal sealed class KeepinClientsSyncTask : ISyncTask
{
    public string Name => "keepin-clients";

    public async Task RunAsync(IServiceProvider services, ILogger logger, CancellationToken cancellationToken)
    {
        var result = await services.SendAsync(new SyncKeepinCrmClientsCommand(DryRun: false, RemoveUnlinked: false), cancellationToken);
        logger.LogInformation(
            "Received {Received}, created {Created}, updated {Updated}, unchanged {Unchanged}, marked not used: {Deactivated}",
            result.Received, result.Created, result.Updated, result.Unchanged,
            result.Deactivated.Count == 0 ? "none" : string.Join("; ", result.Deactivated));
    }
}
