using Microsoft.Extensions.Logging;
using Reference.Application.Features.Supplier.Commands.SyncKeepinCrmSuppliers;

namespace Host.Seed.Jobs.Sync.Keepin;

/// <summary>KeepinCRM → Brevi supplier refresh, the same use case as the admin «Оновити з KeepinCRM» button.</summary>
internal sealed class KeepinSuppliersSyncTask : ISyncTask
{
    public string Name => "keepin-suppliers";

    public async Task RunAsync(IServiceProvider services, ILogger logger, CancellationToken cancellationToken)
    {
        var result = await services.SendAsync(new SyncKeepinCrmSuppliersCommand(), cancellationToken);
        logger.LogInformation(
            "Received {Received}, created {Created}, linked {Linked}, updated {Updated}, missing in KeepinCRM: {Missing}",
            result.Received, result.Created, result.Linked, result.Updated,
            result.MissingInKeepinCrm.Count == 0 ? "none" : string.Join("; ", result.MissingInKeepinCrm));
    }
}
