using Catalog.Application.Contracts.Pricing.Ppe.SupplierPrices;
using Catalog.Application.Features.Pricing.Ppe.SupplierPrices.Import;
using Catalog.Application.Features.Pricing.Ppe.SupplierPrices.Running;
using Catalog.Application.Features.Pricing.Ppe.SupplierPrices.Sources;

namespace Catalog.Application.Features.Pricing.Ppe.SupplierPrices.Refresh;

/// <summary>The outcome of one source in a scheduled run: its result, or the reason it changed nothing.</summary>
public sealed record PpeScheduledRefreshOutcome(string SourceKey, PpePriceImportResult? Result, string? Error);

/// <summary>
/// The daily job's step: on its day (Monday for АВ-Центр, 3C-Маркет and Перчатка-центр; every day for the Insight
/// and Трєле cabinets; Kyiv time) each connected source is read and its changed prices are saved, recorded as «Розклад». A source that cannot be read changes nothing,
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

    /// <summary>The sources due today; a source due weekly runs only on Kyiv Monday.</summary>
    public IReadOnlyList<PpePriceSource> DueToday()
    {
        var day = TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), Kyiv).DayOfWeek;
        return PpePriceSources.All.Where(source => source.CanRefresh && source.Cadence switch
        {
            PpePriceUpdateCadence.Daily => true,
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
