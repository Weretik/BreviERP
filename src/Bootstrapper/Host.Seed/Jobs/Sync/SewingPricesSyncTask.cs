using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Catalog.Application.Features.Pricing.Shared.Sewing.AutoPublication;

namespace Host.Seed.Jobs.Sync;

/// <summary>
/// Nightly safety net of automatic Sewing price publication (CAT-012): publishes every Sewing model whose live price
/// differs from the published one, for example after a change saved outside the API or a failed automatic run.
/// Runs after the KeepinCRM steps; on a normal night it publishes nothing.
/// </summary>
internal sealed class SewingPricesSyncTask : ISyncTask
{
    public const string Actor = "nightly-sync";

    public string Name => "sewing-prices";

    public async Task RunAsync(IServiceProvider services, ILogger logger, CancellationToken cancellationToken)
    {
        var report = await services.GetRequiredService<SewingPricePublisher>()
            .PublishChangedAsync(null, Actor, cancellationToken);
        logger.LogInformation(
            "Published {Published} articles, unchanged {Unchanged}, not calculable {Blocked}, skipped models: {Skipped}",
            report.Published.Count, report.Unchanged, report.Blocked.Count,
            report.Skipped.Count == 0 ? "none" : string.Join(", ", report.Skipped));
        foreach (var blocked in report.Blocked.GroupBy(x => x.ModelCode ?? x.ProductId.ToString()))
            logger.LogWarning("{Model}: no price for {Articles}: {Blockers}", blocked.Key,
                string.Join(", ", blocked.Select(x => x.SaleSku)),
                string.Join(", ", blocked.SelectMany(x => x.Blockers).Distinct()));
    }
}
