using Catalog.Application.Contracts.Persistence.ProductOverview;
using Catalog.Infrastructure.DataBase;

namespace Catalog.Infrastructure.Products.Overview;

/// <summary>
/// One query for the fabric variants of the «Товари» rows: each Article with its own fabric line (else the model's
/// shared one) and the prices of its latest publication.
/// </summary>
public sealed class ProductOverviewVariantReader(CatalogDbContext db) : IProductOverviewVariantReader
{
    public async Task<IReadOnlyList<ProductOverviewVariantRow>> ReadAsync(
        IReadOnlyCollection<int> productIds, CancellationToken cancellationToken)
    {
        if (productIds.Count == 0)
            return [];
        var ids = productIds.ToArray();
        return await db.Database.SqlQuery<ProductOverviewVariantRow>($"""
            SELECT a."ProductId", a."Id" AS "ArticleId", a."SaleSku", a."IsActive", a."SortOrder",
                COALESCE(own."FabricId", shared."FabricId") AS "FabricId",
                pub."Retail", pub."Wholesale", pub."LargeWholesale", pub."PublishedAt"
            FROM catalog."SewingArticles" a
            LEFT JOIN LATERAL (
                SELECT m."FabricId" FROM catalog."SewingMaterialLines" m
                WHERE m."ProductId" = a."ProductId" AND m."ArticleId" = a."Id" AND m."Kind" = 1
                ORDER BY m."SortOrder", m."Id" LIMIT 1) own ON TRUE
            LEFT JOIN LATERAL (
                SELECT m."FabricId" FROM catalog."SewingMaterialLines" m
                WHERE m."ProductId" = a."ProductId" AND m."ArticleId" IS NULL AND m."Kind" = 1
                ORDER BY m."SortOrder", m."Id" LIMIT 1) shared ON TRUE
            LEFT JOIN LATERAL (
                SELECT (c."SnapshotJson"->'calculation'->'prices'->>'retail')::numeric AS "Retail",
                    (c."SnapshotJson"->'calculation'->'prices'->>'wholesale')::numeric AS "Wholesale",
                    (c."SnapshotJson"->'calculation'->'prices'->>'largeWholesale')::numeric AS "LargeWholesale",
                    p."PublishedAt"
                FROM catalog."SewingPricePublications" p
                JOIN catalog."SewingCalculations" c ON c."Id" = p."CalculationId"
                WHERE c."ProductId" = a."ProductId" AND c."ArticleId" = a."Id"
                ORDER BY p."Id" DESC LIMIT 1) pub ON TRUE
            WHERE a."ProductId" = ANY({ids})
            """).ToListAsync(cancellationToken);
    }
}
