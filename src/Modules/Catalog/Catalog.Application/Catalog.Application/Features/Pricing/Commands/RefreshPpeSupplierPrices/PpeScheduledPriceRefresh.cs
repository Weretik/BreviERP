using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Features.Pricing.Commands.ImportPpeSupplierFiles;
using Catalog.Application.Features.Pricing.Shared.Ppe.SupplierPrices.Running;
using Catalog.Application.Features.Pricing.Shared.Ppe.SupplierPrices.Sources;

namespace Catalog.Application.Features.Pricing.Commands.RefreshPpeSupplierPrices;

/// <summary>The outcome of one source in a scheduled run: its result, or the reason it changed nothing.</summary>
public sealed record PpeScheduledRefreshOutcome(string SourceKey, PpePriceImportResult? Result, string? Error);

/// <summary>
/// The step of the brevierp-sync job (Cloud Scheduler, Mon–Fri 09:00 Kyiv): on Kyiv Monday АВ-Центр, 3C-Маркет,
/// Перчатка-центр and the Трєле cabinet each connected source is read and its changed prices are saved, recorded as «Розклад». A source that cannot be read changes nothing,
/// its reason is recorded as the run's problem so «Імпорт цін» shows it, and the next source still runs.
/// </summary>
public sealed class PpeScheduledPriceRefresh(
    IPpeSupplierOnlinePriceReader reader,
    PpePriceImportRunner runner,
    IPpePriceSourceStore store,
    TimeProvider timeProvider)
{
    public const string Trigger = "Розклад";
    private static readonly TimeZoneInfo Kyiv = TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv");

    /// <summary>The sources due today: every connected source runs only on Kyiv Monday.</summary>
    public IReadOnlyList<PpePriceSource> DueToday()
    {
        var day = TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), Kyiv).DayOfWeek;
        return PpePriceSources.All.Where(source => source.CanRefresh && source.Cadence switch
        {
            PpePriceUpdateCadence.WeeklyMonday => day == DayOfWeek.Monday,
            _ => false
        }).ToList();
    }

    public async Task<IReadOnlyList<PpeScheduledRefreshOutcome>> RunDueAsync(string actor, CancellationToken cancellationToken)
    {
        var outcomes = new List<PpeScheduledRefreshOutcome>();
        foreach (var source in DueToday())
        {
            var result = await runner.RunAsync(source, (products, token) => reader.ReadAsync(source, products, token),
                new PpePriceImportRequest(false, Trigger, actor, actor), cancellationToken);
            if (result.IsSuccess)
            {
                outcomes.Add(new PpeScheduledRefreshOutcome(source.Key, result.Value, null));
                continue;
            }

            var error = string.Join(" ", result.ValidationErrors.Select(x => x.ErrorMessage));
            await store.RecordRunAsync(new PpePriceImportRun(source.Key, Trigger, actor, timeProvider.GetUtcNow(),
                0, 0, 0, 0, [new PpePriceImportProblem(null, null, error)]), cancellationToken);
            outcomes.Add(new PpeScheduledRefreshOutcome(source.Key, null, error));
        }

        return outcomes;
    }
}
