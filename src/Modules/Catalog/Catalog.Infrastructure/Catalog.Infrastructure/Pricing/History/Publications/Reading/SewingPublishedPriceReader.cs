using Catalog.Application.Contracts.Pricing;
using Catalog.Domain.Products.Enums;
using Catalog.Infrastructure.DataBase;

namespace Catalog.Infrastructure.Pricing.History.Publications.Reading;

public sealed class SewingPublishedPriceReader(CatalogDbContext db) : ISewingPublishedPriceReader
{
    public async Task<IReadOnlyList<SewingPublishedProductPrices>> ReadAsync(
        IReadOnlyCollection<int> productIds, CancellationToken cancellationToken)
    {
        var ids = productIds.ToArray();
        var products = await db.Products.FromSqlInterpolated(
                $"SELECT * FROM catalog.\"Products\" WHERE \"Id\" = ANY({ids})").AsNoTracking()
            .Where(x => !x.IsDeleted && x.Type == ProductType.Sewing
                && (x.CatalogProfile == null || x.CatalogProfile.IsActive))
            .Select(x => new { ProductId = x.Id.Value, ArticleCount = x.Articles.Count(a => a.IsActive) })
            .ToListAsync(cancellationToken);
        if (products.Count == 0)
            return [];
        var activeIds = products.Select(x => x.ProductId).ToArray();
        var rows = await db.Database.SqlQuery<SewingPublishedArticlePriceRow>($"""
            SELECT DISTINCT ON (c."ArticleId") c."ProductId", c."ArticleId", a."SaleSku",
                p."PublicId" AS "PublicationId", c."PublicId" AS "CalculationId", p."PublishedAt",
                (c."SnapshotJson"->'calculation'->'prices'->>'retail')::numeric AS "Retail",
                (c."SnapshotJson"->'calculation'->'prices'->>'wholesale')::numeric AS "Wholesale",
                (c."SnapshotJson"->'calculation'->'prices'->>'largeWholesale')::numeric AS "LargeWholesale"
            FROM catalog."SewingPricePublications" p
            JOIN catalog."SewingCalculations" c ON c."Id" = p."CalculationId"
            JOIN catalog."SewingArticles" a ON a."Id" = c."ArticleId" AND a."ProductId" = c."ProductId"
            WHERE c."ProductId" = ANY({activeIds}) AND a."IsActive"
            ORDER BY c."ArticleId", p."Id" DESC
            """).ToListAsync(cancellationToken);
        var byProduct = rows.ToLookup(x => x.ProductId);
        return products.OrderBy(x => x.ProductId).Select(product => new SewingPublishedProductPrices(
            product.ProductId, product.ArticleCount,
            byProduct[product.ProductId].OrderBy(x => x.ArticleId).Select(row => new SewingPublishedArticlePrice(
                row.ArticleId, row.SaleSku, row.PublicationId, row.CalculationId, row.PublishedAt,
                new(row.Retail, row.Wholesale, row.LargeWholesale))).ToArray())).ToArray();
    }
}
