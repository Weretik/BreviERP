using Ardalis.Result;
using Catalog.Application.Contracts.Persistence.SheetImport;
using Catalog.Application.Contracts.Pricing.Ppe;
using Catalog.Application.Features.Product.SheetImport.Planning;
using Catalog.Domain.Products.Entities;
using Catalog.Domain.Products.ValueObjects;
using Catalog.Infrastructure.DataBase;
using Catalog.Infrastructure.Pricing.History;
using Npgsql;

namespace Catalog.Infrastructure.Products.SheetImport;

/// <summary>
/// Writes a sheet import plan in one transaction: deletes (with the deleted products' PPE offer versions) are saved
/// first so that new rows never clash with old ones on the unique name, slug, model code or sale SKU; then cards
/// and articles (articles get their IDs), then compositions, then PPE offer versions through the offer writer.
/// </summary>
internal sealed class SheetProductImportStore(CatalogDbContext db, IPpeOfferVersionWriter offers)
    : ISheetProductImportStore
{
    public async Task<SheetImportStoredState> LoadAsync(CancellationToken cancellationToken)
    {
        var products = await db.Products
            .Include(x => x.CatalogProfile)
            .Include(x => x.Articles)
            .Include(x => x.MaterialLines)
            .Include(x => x.OperationLines)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
        var calculations = await db.Set<SavedSewingCalculationRecord>().AsNoTracking()
            .Select(x => new { x.ProductId, x.ArticleId }).Distinct().ToListAsync(cancellationToken);
        return new SheetImportStoredState(products,
            calculations.Select(x => x.ProductId.Value).ToHashSet(),
            calculations.Select(x => x.ArticleId.Value).ToHashSet());
    }

    public async Task<Result> ApplyAsync(SheetImportPlan plan, string actor, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        await using var transaction = await db.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.ReadCommitted, cancellationToken);
        try
        {
            if (plan.Deletes.Count > 0)
            {
                var ids = plan.Deletes.Select(x => x.Id.Value).ToArray();
                await db.Database.ExecuteSqlInterpolatedAsync(
                    $"DELETE FROM catalog.\"PpePricingOfferVersions\" WHERE \"ProductId\" = ANY({ids})", cancellationToken);
                db.Products.RemoveRange(plan.Deletes);
                await db.SaveChangesAsync(cancellationToken);
            }

            var products = new List<(Product Product, SheetPlannedProduct Planned)>();
            foreach (var planned in plan.Writes)
            {
                var product = planned.Existing ?? Product.Create(planned.Id, planned.Draft.Name, planned.Draft.Name,
                    ProductSlug.Create(planned.Slug), planned.Draft.Type, now);
                SheetProductApplier.ApplyCard(product, planned.Draft, now);
                if (planned.Existing is null) db.Products.Add(product);
                products.Add((product, planned));
            }
            await db.SaveChangesAsync(cancellationToken);

            foreach (var (product, planned) in products.Where(x => x.Planned.Draft.Sewing is not null))
                SheetProductApplier.ApplyComposition(product, planned.Draft.Sewing!, now);
            await db.SaveChangesAsync(cancellationToken);

            foreach (var planned in plan.Writes.Where(x => x.Offer is not null))
            {
                var appended = await offers.AppendAsync(planned.Id.Value, planned.Offer!.ExpectedVersionId,
                    Guid.NewGuid(), planned.Offer.Values, actor, cancellationToken);
                if (!appended.IsSuccess)
                    return await RollBackAsync(transaction,
                        $"Ціни ЗІЗ {planned.Draft.Code} змінилися під час імпорту. Повторіть попередній перегляд.", cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException)
        {
            return await RollBackAsync(transaction, "Товари змінилися під час імпорту. Повторіть попередній перегляд.", cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException postgres)
        {
            return await RollBackAsync(transaction,
                $"База даних не прийняла зміни ({postgres.ConstraintName ?? postgres.SqlState}). Нічого не збережено.", cancellationToken);
        }
    }

    private async Task<Result> RollBackAsync(
        Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction, string message, CancellationToken cancellationToken)
    {
        await transaction.RollbackAsync(cancellationToken);
        db.ChangeTracker.Clear();
        return Result.Conflict(message);
    }
}
