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
        => db.Products.Include(x => x.Articles).Include(x => x.MaterialLines).Include(x => x.OperationLines)
            .AsSplitQuery().SingleOrDefaultAsync(x => x.Id == ProductId.Create(productId) && !x.IsDeleted && x.Type == ProductType.Sewing, cancellationToken);

    public Task<bool> SaleSkuExistsAsync(string saleSku, CancellationToken cancellationToken)
        => db.Set<SewingArticle>().AnyAsync(x => x.SaleSku == saleSku, cancellationToken);

    public Task<SewingArticleList?> ReadArticlesAsync(int productId, CancellationToken cancellationToken)
        => ArticlesQuery(productId).SingleOrDefaultAsync(cancellationToken);

    internal IQueryable<SewingArticleList> ArticlesQuery(int productId)
        => db.Products.AsNoTracking().Where(x => x.Id == ProductId.Create(productId) && !x.IsDeleted && x.Type == ProductType.Sewing)
            .Select(x => new SewingArticleList(x.Id.Value, x.ModelCode, x.CompositionVersion,
                x.Articles.OrderBy(a => a.SortOrder).ThenBy(a => a.Id).Select(a =>
                    new SewingArticleView(a.Id.Value, a.ProductId.Value, a.SaleSku, a.IsActive, a.SortOrder)).ToList()));

    public async Task<SewingComposition?> ReadCompositionAsync(int productId, long? articleId, CancellationToken cancellationToken)
    {
        var row = await CompositionQuery(productId, articleId).SingleOrDefaultAsync(cancellationToken);
        if (row is null) return null;
        var blockers = new List<string>();
        if (!row.Active) blockers.Add("Article is inactive.");
        blockers.AddRange(row.Materials.Where(m => m.Required && !m.Reviewed).Select(m => $"Required material line {m.Id} has not been reviewed."));
        blockers.AddRange(row.Operations.Where(o => !o.Reviewed).Select(o => $"Required operation line {o.Id} has not been reviewed."));
        blockers.AddRange(row.Materials.Where(m => m.Kind == "OtherCost").Select(m => $"Other cost line {m.Id} has no costing reference owner."));
        if (!row.Materials.Any(m => m.Kind == "Fabric")) blockers.Add("No effective fabric line is configured.");
        if (row.Operations.Count == 0) blockers.Add("No operation lines are configured.");
        return new SewingComposition(productId, articleId, row.ModelCode, row.CompositionVersion, row.Materials, row.Operations, blockers);
    }

    internal IQueryable<CompositionReadRow> CompositionQuery(int productId, long? articleId)
    {
        var selectedId = articleId.HasValue ? ArticleId.Create(articleId.Value) : (ArticleId?)null;
        return db.Products.AsNoTracking()
            .Where(x => x.Id == ProductId.Create(productId) && !x.IsDeleted && x.Type == ProductType.Sewing)
            .Where(x => !selectedId.HasValue || x.Articles.Any(a => a.Id == selectedId.Value))
            .Select(x => new CompositionReadRow(
                x.ModelCode, x.CompositionVersion,
                !selectedId.HasValue || x.Articles.Any(a => a.Id == selectedId.Value && a.IsActive),
                x.MaterialLines.Where(m => !m.ArticleId.HasValue || m.ArticleId == selectedId)
                    .OrderBy(m => m.SortOrder).ThenBy(m => m.Id)
                    .Select(m => new SewingMaterialView(m.Id.Value, m.ArticleId.HasValue ? m.ArticleId.Value.Value : null,
                        m.ArticleId.HasValue ? "Article" : "Model", m.Kind.ToString(),
                        m.FabricId.HasValue ? m.FabricId.Value.Value : m.AccessoryId.HasValue ? m.AccessoryId.Value.Value : null,
                        m.CostCode, m.Quantity, m.Unit.ToString(), m.IsRequired, m.SortOrder, m.IsReviewed,
                        m.ReviewedBy, m.ReviewedAt)).ToList(),
                x.OperationLines.OrderBy(o => o.SortOrder).ThenBy(o => o.Id)
                    .Select(o => new SewingOperationView(o.Id.Value, o.OperationId.Value, o.Quantity, o.ManualMinutes,
                        o.SortOrder, o.IsReviewed, o.ReviewedBy, o.ReviewedAt)).ToList()
            )).AsSplitQuery();
    }

    internal sealed record CompositionReadRow(string? ModelCode, long CompositionVersion, bool Active,
        List<SewingMaterialView> Materials, List<SewingOperationView> Operations);

    public async Task<bool> SaveAsync(CancellationToken cancellationToken)
    {
        try { await db.SaveChangesAsync(cancellationToken); return true; }
        catch (DbUpdateConcurrencyException) { db.ChangeTracker.Clear(); return false; }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        { db.ChangeTracker.Clear(); return false; }
    }
}
