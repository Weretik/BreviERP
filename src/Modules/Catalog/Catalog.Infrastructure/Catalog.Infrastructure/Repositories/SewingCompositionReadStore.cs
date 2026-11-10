using Catalog.Application.Features.Product.Sewing;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.ValueObjects;
using Catalog.Infrastructure.DataBase;

namespace Catalog.Infrastructure.Repositories;

internal sealed class SewingCompositionReadStore(CatalogDbContext db) : ISewingCompositionReadStore, ISewingCompositionListReader
{
    public async Task<IReadOnlyList<SewingCompositionListRow>> ReadAllAsync(CancellationToken cancellationToken)
        => await db.Products.AsNoTracking()
            .Where(product => !product.IsDeleted && product.Type == ProductType.Sewing)
            .OrderBy(product => product.ModelCode).ThenBy(product => product.Id)
            .Select(product => new SewingCompositionListRow(product.Id.Value, product.ModelCode,
                product.Articles.OrderBy(article => article.SortOrder).ThenBy(article => article.Id)
                    .Select(article => new SewingArticleView(article.Id.Value, article.ProductId.Value,
                        article.SaleSku, article.IsActive, article.SortOrder, null, null)).ToList(),
                product.MaterialLines.OrderBy(line => line.SortOrder).ThenBy(line => line.Id)
                    .Select(line => new SewingMaterialView(line.Id.Value,
                        line.ArticleId.HasValue ? line.ArticleId.Value.Value : null,
                        line.ArticleId.HasValue ? "Article" : "Model", line.Kind.ToString(),
                        line.FabricId.HasValue ? line.FabricId.Value.Value
                            : line.AccessoryId.HasValue ? line.AccessoryId.Value.Value : null,
                        line.CostCode, line.Quantity, line.Unit.ToString(), line.SortOrder, line.Comment)).ToList()))
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

    public Task<SewingArticleList?> ReadArticlesAsync(int productId, CancellationToken cancellationToken)
        => ArticlesQuery(productId).SingleOrDefaultAsync(cancellationToken);

    internal IQueryable<SewingArticleList> ArticlesQuery(int productId)
        => db.Products.AsNoTracking()
            .Where(product => product.Id == ProductId.Create(productId)
                && !product.IsDeleted && product.Type == ProductType.Sewing)
            .Select(product => new SewingArticleList(product.Id.Value, product.ModelCode,
                product.CompositionVersion,
                product.Articles.OrderBy(article => article.SortOrder).ThenBy(article => article.Id)
                    .Select(article => new SewingArticleView(article.Id.Value, article.ProductId.Value,
                        article.SaleSku, article.IsActive, article.SortOrder,
                        // The variant's own fabric line (CAT-015); null when it has none yet.
                        product.MaterialLines.Where(line => line.ArticleId == article.Id)
                            .Select(line => line.FabricId.HasValue ? (int?)line.FabricId.Value.Value : null)
                            .FirstOrDefault(),
                        product.MaterialLines.Where(line => line.ArticleId == article.Id)
                            .Select(line => (decimal?)line.Quantity).FirstOrDefault())).ToList()));

    public async Task<SewingCompositionReadSnapshot?> ReadCompositionAsync(int productId, long? articleId,
        CancellationToken cancellationToken)
    {
        var row = await CompositionQuery(productId, articleId).SingleOrDefaultAsync(cancellationToken);
        if (row is null)
            return null;

        return new SewingCompositionReadSnapshot(productId, articleId, row.ModelCode, row.CompositionVersion,
            row.ArticleIsActive, row.Materials, row.Operations);
    }

    internal IQueryable<CompositionReadRow> CompositionQuery(int productId, long? articleId)
    {
        var selectedId = articleId.HasValue ? ArticleId.Create(articleId.Value) : (ArticleId?)null;
        return db.Products.AsNoTracking()
            .Where(product => product.Id == ProductId.Create(productId)
                && !product.IsDeleted && product.Type == ProductType.Sewing)
            .Where(product => !selectedId.HasValue
                || product.Articles.Any(article => article.Id == selectedId.Value))
            .Select(product => new CompositionReadRow(
                product.ModelCode,
                product.CompositionVersion,
                !selectedId.HasValue || product.Articles.Any(article => article.Id == selectedId.Value && article.IsActive),
                product.MaterialLines.Where(line => !line.ArticleId.HasValue || line.ArticleId == selectedId)
                    .OrderBy(line => line.SortOrder).ThenBy(line => line.Id)
                    .Select(line => new SewingMaterialView(line.Id.Value,
                        line.ArticleId.HasValue ? line.ArticleId.Value.Value : null,
                        line.ArticleId.HasValue ? "Article" : "Model", line.Kind.ToString(),
                        line.FabricId.HasValue ? line.FabricId.Value.Value
                            : line.AccessoryId.HasValue ? line.AccessoryId.Value.Value : null,
                        line.CostCode, line.Quantity, line.Unit.ToString(), line.SortOrder, line.Comment)).ToList(),
                product.OperationLines.OrderBy(line => line.SortOrder).ThenBy(line => line.Id)
                    .Select(line => new SewingOperationView(line.Id.Value, line.OperationId.Value,
                        line.Quantity, line.ManualMinutes, line.SortOrder, line.Comment)).ToList()))
            .AsSplitQuery();
    }
}
