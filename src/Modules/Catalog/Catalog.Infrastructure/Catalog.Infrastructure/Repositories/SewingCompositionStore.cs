using Catalog.Application.Features.Product.Sewing;
using Catalog.Domain.Products.Entities;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.ValueObjects;
using Catalog.Infrastructure.DataBase;
using Npgsql;

namespace Catalog.Infrastructure.Repositories;

internal sealed class SewingCompositionStore(CatalogDbContext db) : ISewingCompositionStore
{
    public Task<Product?> LoadAsync(int productId, CancellationToken cancellationToken)
        => db.Products.Include(product => product.Articles)
            .Include(product => product.MaterialLines)
            .Include(product => product.OperationLines)
            .AsSplitQuery()
            .SingleOrDefaultAsync(product => product.Id == ProductId.Create(productId)
                && !product.IsDeleted && product.Type == ProductType.Sewing, cancellationToken);

    public async Task<bool> SaleSkuExistsAsync(string saleSku, CancellationToken cancellationToken)
        => await db.Set<SewingArticle>().AnyAsync(article => article.SaleSku == saleSku, cancellationToken)
            || await db.Set<ProductCatalogProfile>().AnyAsync(profile => profile.SaleSku == saleSku, cancellationToken);

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
