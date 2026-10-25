using Catalog.Application.Contracts.Pricing;
using Catalog.Domain.Products.ValueObjects;
using Catalog.Infrastructure.DataBase;

namespace Catalog.Infrastructure.Pricing.History.Calculations;

public sealed class SewingCalculationStore(CatalogDbContext db) : ISewingCalculationStore
{
    public async Task<SavedSewingCalculation?> FindRequestAsync(Guid requestId, CancellationToken cancellationToken)
    {
        var row = await db.Set<SavedSewingCalculationRecord>().AsNoTracking()
            .SingleOrDefaultAsync(x => x.RequestId == requestId, cancellationToken);
        return row is null ? null : SewingCalculationSnapshotSerializer.Read(row);
    }

    public async Task<SavedSewingCalculation?> FindAsync(
        int productId, long articleId, Guid id, CancellationToken cancellationToken)
    {
        if (productId <= 0 || articleId <= 0)
            return null;
        var row = await db.Set<SavedSewingCalculationRecord>().AsNoTracking()
            .Where(x => x.ProductId == ProductId.Create(productId) && x.ArticleId == ArticleId.Create(articleId))
            .SingleOrDefaultAsync(x => x.PublicId == id, cancellationToken);
        return row is null ? null : SewingCalculationSnapshotSerializer.Read(row);
    }

    public async Task SaveAsync(SavedSewingCalculation calculation, CancellationToken cancellationToken)
    {
        db.Add(new SavedSewingCalculationRecord
        {
            PublicId = calculation.Id,
            RequestId = calculation.RequestId,
            CreatedAt = calculation.CreatedAt,
            CreatedBy = calculation.CreatedBy,
            SnapshotJson = SewingCalculationSnapshotSerializer.Serialize(calculation.Snapshot),
            ProductId = ProductId.Create(calculation.Snapshot.ProductId),
            ArticleId = ArticleId.Create(calculation.Snapshot.ArticleId)
        });
        await db.SaveChangesAsync(cancellationToken);
    }
}
