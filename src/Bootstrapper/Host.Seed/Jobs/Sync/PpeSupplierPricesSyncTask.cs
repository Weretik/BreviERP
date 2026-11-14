using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Catalog.Application.Features.Pricing.Commands.RefreshPpeSupplierPrices;

namespace Host.Seed.Jobs.Sync;

/// <summary>
/// Scheduled PPE supplier prices of «Імпорт цін» (CAT-017 stages 2–3): on Kyiv Monday (brevierp-sync runs
/// Mon–Fri 09:00) АВ-Центр and Перчатка-центр pages, the 3C-Маркет lists and the Трєле dealer cabinet; other days nothing; the changed prices
/// are saved. A source that cannot be read is logged and recorded with its reason; the step then fails so the job
/// shows it.
/// </summary>
internal sealed class PpeSupplierPricesSyncTask : ISyncTask
{
    public const string Actor = "nightly-sync";

    public string Name => "ppe-supplier-prices";

    public async Task RunAsync(IServiceProvider services, ILogger logger, CancellationToken cancellationToken)
    {
        var refresh = services.GetRequiredService<PpeScheduledPriceRefresh>();
        if (refresh.DueToday().Count == 0)
        {
            logger.LogInformation("No supplier price source is due today");
            return;
        }

        var outcomes = await refresh.RunDueAsync(Actor, cancellationToken);
        foreach (var outcome in outcomes)
        {
            if (outcome.Result is { } result)
                logger.LogInformation("{Source}: checked {Checked}, changed {Changed}, unchanged {Unchanged}, skipped {Skipped}, problems {Problems}",
                    outcome.SourceKey, result.Checked, result.Changed, result.Unchanged, result.Skipped,
                    result.Checked - result.Changed - result.Unchanged - result.Skipped);
            else
                logger.LogWarning("{Source}: nothing changed: {Reason}", outcome.SourceKey, outcome.Error);
        }

        var failed = outcomes.Where(x => x.Error is not null).Select(x => x.SourceKey).ToList();
        if (failed.Count > 0)
            throw new InvalidOperationException($"Supplier prices not read: {string.Join(", ", failed)}.");
    }
}
