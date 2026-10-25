using Catalog.Application.Contracts.Pricing;
using Catalog.Domain.Products.ValueObjects;
using Catalog.Infrastructure.DataBase;
using Catalog.Infrastructure.Pricing.History.Calculations;

namespace Catalog.Infrastructure.Pricing.History.Publications;

public sealed class SewingPublicationStore(CatalogDbContext db) : ISewingPublicationStore
{
    public async Task<PublishedSewingPrice?> FindPublicationAsync(Guid calculationId, CancellationToken cancellationToken)
    {
        var row = await db.Set<SewingPricePublicationRecord>().AsNoTracking().Include(x => x.Calculation)
            .SingleOrDefaultAsync(x => x.Calculation.PublicId == calculationId, cancellationToken);
        return row is null ? null : Map(row);
    }

    public async Task<PublishedSewingPrice?> ReadCurrentAsync(int productId, long articleId, CancellationToken cancellationToken)
    {
        if (productId <= 0 || articleId <= 0)
            return null;
        var calculations = db.Set<SavedSewingCalculationRecord>().AsNoTracking()
            .Where(x => x.ProductId == ProductId.Create(productId) && x.ArticleId == ArticleId.Create(articleId));
        var row = await db.Set<SewingPricePublicationRecord>().AsNoTracking().Include(x => x.Calculation)
            .Where(x => calculations.Any(c => c.Id == x.CalculationId))
            .OrderByDescending(x => x.Id).FirstOrDefaultAsync(cancellationToken);
        return row is null ? null : Map(row);
    }

    public async Task PublishAsync(PublishedSewingPrice publication, CancellationToken cancellationToken)
    {
        var calculationId = await db.Set<SavedSewingCalculationRecord>()
            .Where(x => x.PublicId == publication.CalculationId).Select(x => x.Id).SingleAsync(cancellationToken);
        db.Add(new SewingPricePublicationRecord
        {
            PublicId = publication.Id,
            CalculationId = calculationId,
            PublishedAt = publication.PublishedAt,
            PublishedBy = publication.PublishedBy
        });
        await db.SaveChangesAsync(cancellationToken);
    }

    private static PublishedSewingPrice Map(SewingPricePublicationRecord row)
    {
        var saved = SewingCalculationSnapshotSerializer.Read(row.Calculation);
        return new(row.PublicId, saved.Id, saved.Snapshot.ProductId, saved.Snapshot.ArticleId,
            row.PublishedAt, row.PublishedBy, saved.Snapshot.Calculation.Prices!);
    }
}
