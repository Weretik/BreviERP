using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Reference.Application.Features.Supplier.KeepinCrmSync;

namespace Host.Seed.Jobs;

/// <summary>
/// --keepin-suppliers-sync=true: the daily KeepinCRM → Brevi supplier refresh. Cloud Scheduler executes the
/// brevierp-keepin-suppliers Cloud Run Job (same image, this argument baked in). Only this mode runs: no
/// migrations or seeding. Safe to rerun. Exit code 1 when KeepinCRM is unavailable; nothing changes then.
/// </summary>
internal static class KeepinSuppliersSyncCommandRunner
{
    public static async Task RunAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("KeepinSuppliersSync");
        logger.LogInformation("KeepinCRM suppliers sync started");
        try
        {
            var result = await scope.ServiceProvider.GetRequiredService<KeepinCrmSupplierSync>().RunAsync(CancellationToken.None);
            logger.LogInformation(
                "KeepinCRM suppliers sync OK: received {Received}, created {Created}, linked {Linked}, updated {Updated}, missing in KeepinCRM: {Missing}",
                result.Received, result.Created, result.Linked, result.Updated,
                result.MissingInKeepinCrm.Count == 0 ? "none" : string.Join("; ", result.MissingInKeepinCrm));
        }
        catch (Exception error)
        {
            logger.LogError("KeepinCRM suppliers sync FAILED: {ErrorType}: {Reason}", error.GetType().Name, error.Message);
            throw;
        }
    }
}
