using Catalog.Application.Contracts.Pricing;
using Catalog.Application.Contracts.Suggestions;
using Catalog.Domain.Products.Entities;
using Catalog.Domain.Products.Enums;
using Catalog.Infrastructure.DataBase;
namespace Catalog.Infrastructure.Suggestions;

public sealed class SewingSuggestionCompositionReader(CatalogDbContext db) : ISewingSuggestionCompositionReader
{
    private const int IdentityLimit = 2000;
    private const int LineLimit = 100000;
    public async Task<SewingSuggestionCompositionBatch> ReadAsync(IReadOnlyCollection<SewingSuggestionIdentity> identities,
        string sourceSeason, CancellationToken ct)
    {
        if (identities.Count == 0) return new([], false);
        if (identities.Count > IdentityLimit) return new([], true);
        var productIds = identities.Select(x => x.ProductId).Distinct().ToArray();
        var articleIds = identities.Select(x => x.ArticleId).Distinct().ToArray();
        var requested = identities.Select(x => (x.ProductId, x.ArticleId)).ToHashSet();
        // Scalar ANY parameters avoid provider issues with nullable typed-ID array converters.
        var states = await db.Products.FromSqlInterpolated($"""SELECT * FROM catalog."Products" WHERE "Id" = ANY({productIds})""")
            .AsNoTracking().Where(x => !x.IsDeleted && x.Type == ProductType.Sewing && x.CatalogProfile != null
                && x.CatalogProfile.IsActive && x.CatalogProfile.IsSourceReviewed && x.CatalogProfile.Season == sourceSeason)
            .SelectMany(x => x.Articles.Where(a => a.IsActive), (p, a) => new { ProductId = p.Id.Value,
                ArticleId = a.Id.Value, a.SaleSku, p.CompositionVersion }).Take(IdentityLimit + 1).ToArrayAsync(ct);
        if (states.Length > IdentityLimit) return new([], true);
        var materials = await db.Set<SewingMaterialLine>().FromSqlInterpolated($"""
            SELECT * FROM catalog."SewingMaterialLines" WHERE "ProductId" = ANY({productIds})
              AND ("ArticleId" IS NULL OR "ArticleId" = ANY({articleIds}))
            """).AsNoTracking().OrderBy(x => x.SortOrder).ThenBy(x => x.Id)
            .Select(x => new SuggestionMaterialProjection(x.ProductId.Value, x.ArticleId.HasValue ? x.ArticleId.Value.Value : null,
                new SewingPricingMaterialRow(x.Id.Value,
                    x.Kind == SewingMaterialKind.Fabric ? "Fabric:" + x.FabricId!.Value.Value
                        : x.Kind == SewingMaterialKind.Accessory ? "Accessory:" + x.AccessoryId!.Value.Value : "OtherCost:" + x.CostCode,
                    x.Quantity, x.Unit.ToString(), x.Kind == SewingMaterialKind.Fabric, x.Kind == SewingMaterialKind.OtherCost,
                    x.IsRequired, x.IsReviewed))).Take(LineLimit + 1).ToArrayAsync(ct);
        if (materials.Length > LineLimit) return new([], true);
        var operations = await db.Set<SewingOperationLine>().FromSqlInterpolated($"""
            SELECT * FROM catalog."SewingOperationLines" WHERE "ProductId" = ANY({productIds})
            """).AsNoTracking().OrderBy(x => x.SortOrder).ThenBy(x => x.Id)
            .Select(x => new SuggestionOperationProjection(x.ProductId.Value,
                new SewingPricingOperationRow(x.Id.Value, x.OperationId.Value, x.Quantity, x.ManualMinutes, x.IsReviewed)))
            .Take(LineLimit + 1).ToArrayAsync(ct);
        if (operations.Length > LineLimit) return new([], true);
        var materialGroups = materials.ToLookup(x => x.ProductId);
        var operationGroups = operations.ToLookup(x => x.ProductId);
        var snapshots = new List<SewingArticlePricingSnapshot>();
        var expandedLines = 0;
        foreach (var state in states.Where(x => requested.Contains((x.ProductId, x.ArticleId))))
        {
            var selectedMaterials = materialGroups[state.ProductId].Where(m => m.ArticleId is null || m.ArticleId == state.ArticleId).Select(m => m.Row).ToArray();
            var selectedOperations = operationGroups[state.ProductId].Select(o => o.Row).ToArray();
            expandedLines += selectedMaterials.Length + selectedOperations.Length;
            if (expandedLines > LineLimit) return new([], true);
            snapshots.Add(new(state.ProductId, state.ArticleId, state.SaleSku, state.CompositionVersion, true, selectedMaterials, selectedOperations));
        }
        return new(snapshots, false);
    }
}
