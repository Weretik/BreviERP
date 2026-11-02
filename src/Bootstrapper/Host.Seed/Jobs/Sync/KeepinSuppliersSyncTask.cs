using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Reference.Application.Features.Supplier.KeepinCrmSync;

namespace Host.Seed.Jobs.Sync;

/// <summary>KeepinCRM → Brevi supplier refresh, the same use case as the admin «Оновити з KeepinCRM» button.</summary>
internal sealed class KeepinSuppliersSyncTask : ISyncTask
{
    public string Name => "keepin-suppliers";

    public async Task RunAsync(IServiceProvider services, ILogger logger, CancellationToken cancellationToken)
    {
        var result = await services.GetRequiredService<KeepinCrmSupplierSync>().RunAsync(cancellationToken);
        logger.LogInformation(
            "Received {Received}, created {Created}, linked {Linked}, updated {Updated}, missing in KeepinCRM: {Missing}",
            result.Received, result.Created, result.Linked, result.Updated,
            result.MissingInKeepinCrm.Count == 0 ? "none" : string.Join("; ", result.MissingInKeepinCrm));
    }
}
