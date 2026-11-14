using Microsoft.Extensions.Logging;
using Catalog.Application.Features.Pricing.Commands.RefreshDuePpeSupplierPrices;

namespace Host.Seed.Jobs.Tasks.Pricing;

/// <summary>
/// Scheduled PPE supplier prices of «Імпорт цін» (CAT-017 stages 2–3): «Імпорт цін» decides which sources are due today
/// (АВ-Центр and Перчатка-центр pages, the 3C-Маркет lists, the Трєле dealer cabinet); other days nothing. The changed
/// prices are saved. A source that cannot be read is recorded with its reason; the task then fails so the admin page
/// shows it.
/// </summary>
internal sealed class PpeSupplierPricesTaskRunner : IScheduledTaskRunner
{
    public const string Actor = "nightly-sync";

    public string Key => "ppe-supplier-prices";

    public async Task<string> RunAsync(IServiceProvider services, ILogger logger, CancellationToken cancellationToken)
    {
        var outcomes = await services.SendAsync(new RefreshDuePpeSupplierPricesCommand(Actor), cancellationToken);
        if (outcomes.Count == 0)
        {
            logger.LogInformation("No supplier price source is due today");
            return "Сьогодні жоден постачальник не за розкладом.";
        }

        foreach (var outcome in outcomes)
        {
            if (outcome.Result is { } result)
                logger.LogInformation("{Source}: checked {Checked}, changed {Changed}, unchanged {Unchanged}, skipped {Skipped}, problems {Problems}",
                    outcome.SourceKey, result.Checked, result.Changed, result.Unchanged, result.Skipped,
                    result.Checked - result.Changed - result.Unchanged - result.Skipped);
            else
                logger.LogWarning("{Source}: nothing changed: {Reason}", outcome.SourceKey, outcome.Error);
        }

        var failed = outcomes.Where(x => x.Error is not null).Select(x => $"{x.SourceKey}: {x.Error}").ToList();
        if (failed.Count > 0)
            throw new InvalidOperationException($"Ціни не прочитано: {string.Join("; ", failed)}.");
        var changed = outcomes.Sum(x => x.Result?.Changed ?? 0);
        return $"Перевірено постачальників {outcomes.Count}, змінено цін {changed}.";
    }
}
