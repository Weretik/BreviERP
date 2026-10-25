using Catalog.Application.Contracts.Pricing;
using Catalog.Domain.Products.Entities;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.ValueObjects;
using Catalog.Infrastructure.DataBase;

namespace Catalog.Infrastructure.Pricing.Reading;

public sealed class SewingPricingInputReader(CatalogDbContext db) : ISewingPricingInputReader
{
    public async Task<SewingArticlePricingSnapshot?> ReadAsync(int productId, long articleId, CancellationToken cancellationToken)
    {
        var productKey = ProductId.Create(productId);
        var articleKey = ArticleId.Create(articleId);
        var identity = await db.Products.AsNoTracking().Where(x => x.Id == productKey && !x.IsDeleted && x.Type == ProductType.Sewing)
            .SelectMany(x => x.Articles.Where(a => a.Id == articleKey), (product, article) =>
                new { article.SaleSku, product.CompositionVersion, Active = article.IsActive && (product.CatalogProfile == null || product.CatalogProfile.IsActive) })
            .SingleOrDefaultAsync(cancellationToken);
        if (identity is null)
            return null;
        var materials = await db.Set<SewingMaterialLine>().AsNoTracking()
            .Where(x => x.ProductId == productKey && (x.ArticleId == null || x.ArticleId == articleKey))
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Id)
            .Select(x => new SewingPricingMaterialRow(x.Id.Value,
                x.Kind == SewingMaterialKind.Fabric ? "Fabric:" + x.FabricId!.Value.Value
                    : x.Kind == SewingMaterialKind.Accessory ? "Accessory:" + x.AccessoryId!.Value.Value
                    : "OtherCost:" + x.CostCode,
                x.Quantity, x.Unit.ToString(), x.Kind == SewingMaterialKind.Fabric, x.Kind == SewingMaterialKind.OtherCost,
                x.IsRequired, x.IsReviewed)).ToListAsync(cancellationToken);
        var operations = await db.Set<SewingOperationLine>().AsNoTracking().Where(x => x.ProductId == productKey)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Id)
            .Select(x => new SewingPricingOperationRow(x.Id.Value, x.OperationId.Value, x.Quantity, x.ManualMinutes, x.IsReviewed))
            .ToListAsync(cancellationToken);
        return new SewingArticlePricingSnapshot(productId, articleId, identity.SaleSku, identity.CompositionVersion,
            identity.Active, materials, operations);
    }
}

