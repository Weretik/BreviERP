using BuildingBlocks.Domain.Results;
using Catalog.Domain.Products;
using Npgsql;
using Catalog.Application.Abstractions;
using Catalog.Infrastructure.Persistence;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.Sewing.ValueObjects;
using Catalog.Domain.Products.ValueObjects;
using Catalog.Infrastructure.Persistence.Configurations.Pricing.Calculations;
using Catalog.Application.Abstractions.Sewing;


namespace Catalog.Infrastructure.Persistence.Repositories.Sewing;

internal sealed class SewingCompositionStore(CatalogDbContext db) : ISewingCompositionStore
{
    public Task<Product?> LoadAsync(int productId, CancellationToken cancellationToken)
        => db.Products.Include(product => product.Articles)
            .Include(product => product.MaterialLines)
            .Include(product => product.OperationLines)
            .AsSplitQuery()
            .SingleOrDefaultAsync(product => product.Id == ProductId.Create(productId)
                && !product.IsDeleted && product.Type == ProductType.Sewing, cancellationToken);

    public async Task<bool> SaleSkuExistsAsync(string saleSku, long? exceptArticleId, CancellationToken cancellationToken)
    {
        var except = exceptArticleId.HasValue ? ArticleId.Create(exceptArticleId.Value) : (ArticleId?)null;
        return await db.Set<SewingArticle>().AnyAsync(article => article.SaleSku == saleSku
                && (except == null || article.Id != except), cancellationToken)
            || await db.Set<ProductCatalogProfile>().AnyAsync(profile => profile.SaleSku == saleSku, cancellationToken);
    }

    public Task<bool> HasPriceHistoryAsync(long articleId, CancellationToken cancellationToken)
        => db.Set<SavedSewingCalculationRecord>().AnyAsync(x => x.ArticleId == ArticleId.Create(articleId), cancellationToken);

    public async Task<DomainResult<bool>> AddArticleAsync(Product product, SewingArticle article,
        FabricReferenceId fabricId, decimal fabricQuantity, DateTimeOffset now, CancellationToken cancellationToken)
    {
        // A broken rule or a lost race leaves the transaction uncommitted, so the article saved first is rolled back.
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var added = product.AddArticle(article, now);
        if (added.IsFailure) return added.Error!;
        if (!await SaveAsync(cancellationToken))
            return false;
        var fabric = product.SetArticleFabric(article.Id, fabricId, fabricQuantity, now);
        if (fabric.IsFailure) return fabric.Error!;
        if (!await SaveAsync(cancellationToken))
            return false;
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    public async Task<bool> SaveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            db.ChangeTracker.Clear();
            return false;
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
        { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            db.ChangeTracker.Clear();
            return false;
        }
    }
}
