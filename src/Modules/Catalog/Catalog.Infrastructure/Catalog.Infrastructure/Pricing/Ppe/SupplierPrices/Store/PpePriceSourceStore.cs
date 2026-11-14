using System.Text.Json;
using Catalog.Infrastructure.DataBase;
using Catalog.Infrastructure.Pricing.Ppe.SupplierPrices.Records;
using Catalog.Application.Abstractions.Pricing;

namespace Catalog.Infrastructure.Pricing.Ppe.SupplierPrices.Store;

public sealed class PpePriceSourceStore(CatalogDbContext db) : IPpePriceSourceStore
{
    public async Task<IReadOnlyList<PpePriceSourceSettings>> ReadSettingsAsync(CancellationToken cancellationToken)
        => await db.Set<PpePriceSourceSettingsRecord>().AsNoTracking()
            .Select(x => new PpePriceSourceSettings(x.Key, x.SupplierId, x.DiscountPercent, x.UpdatedAt, x.UpdatedBy))
            .ToListAsync(cancellationToken);

    public async Task SaveSettingsAsync(PpePriceSourceSettings settings, CancellationToken cancellationToken)
    {
        var record = await db.Set<PpePriceSourceSettingsRecord>()
            .SingleOrDefaultAsync(x => x.Key == settings.Key, cancellationToken);
        if (record is null)
        {
            record = new PpePriceSourceSettingsRecord { Key = settings.Key };
            db.Add(record);
        }

        record.SupplierId = settings.SupplierId;
        record.DiscountPercent = settings.DiscountPercent;
        record.UpdatedAt = settings.UpdatedAt;
        record.UpdatedBy = settings.UpdatedBy;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PpePriceImportRun>> ReadLastRunsAsync(CancellationToken cancellationToken)
    {
        var runs = db.Set<PpePriceImportRunRecord>().AsNoTracking();
        var latest = await runs
            .Where(run => run.Id == runs.Where(other => other.SourceKey == run.SourceKey).Max(other => other.Id))
            .ToListAsync(cancellationToken);
        return latest.Select(ToRun).ToList();
    }

    public async Task RecordRunAsync(PpePriceImportRun run, CancellationToken cancellationToken)
    {
        db.Add(new PpePriceImportRunRecord
        {
            SourceKey = run.SourceKey,
            Trigger = run.Trigger,
            Actor = run.Actor,
            FinishedAt = run.FinishedAt,
            Checked = run.Checked,
            Changed = run.Changed,
            Unchanged = run.Unchanged,
            Skipped = run.Skipped,
            ProblemsJson = JsonSerializer.Serialize(run.Problems)
        });
        await db.SaveChangesAsync(cancellationToken);
    }

    private static PpePriceImportRun ToRun(PpePriceImportRunRecord record)
        => new(record.SourceKey, record.Trigger, record.Actor, record.FinishedAt, record.Checked, record.Changed,
            record.Unchanged, record.Skipped,
            JsonSerializer.Deserialize<List<PpePriceImportProblem>>(record.ProblemsJson) ?? []);
}
