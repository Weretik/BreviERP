using Microsoft.Extensions.Logging;
using Reference.Application.Features.Supplier.Commands.SyncKeepinCrmSuppliers;

namespace Host.Seed.Jobs.Tasks.Keepin;

/// <summary>KeepinCRM → Brevi supplier refresh, the same use case as the admin «Оновити з KeepinCRM» button.</summary>
internal sealed class KeepinSuppliersTaskRunner : IScheduledTaskRunner
{
    public string Key => "keepin-suppliers";

    public async Task<string> RunAsync(IServiceProvider services, ILogger logger, CancellationToken cancellationToken)
    {
        var result = await services.SendAsync(new SyncKeepinCrmSuppliersCommand(), cancellationToken);
        logger.LogInformation(
            "Received {Received}, created {Created}, linked {Linked}, updated {Updated}, missing in KeepinCRM: {Missing}",
            result.Received, result.Created, result.Linked, result.Updated,
            result.MissingInKeepinCrm.Count == 0 ? "none" : string.Join("; ", result.MissingInKeepinCrm));
        return $"Отримано {result.Received}: створено {result.Created}, пов'язано {result.Linked}, оновлено {result.Updated}.";
    }
}
