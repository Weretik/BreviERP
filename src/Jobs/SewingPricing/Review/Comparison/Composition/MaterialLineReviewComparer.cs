using Catalog.Domain.Products.Entities;

namespace SewingPricing.Jobs.Review;

internal static class MaterialLineReviewComparer
{
    public static IReadOnlyList<PricingReviewCheck> Compare(SewingMaterialLine expected, SewingMaterialLine? actual) =>
        PricingReviewComparison.Fields(Fields(expected), actual is null ? null : Fields(actual));

    private static object Fields(SewingMaterialLine line) => new
    {
        ProductId = line.ProductId.Value,
        ArticleId = line.ArticleId?.Value,
        line.Kind,
        FabricId = line.FabricId?.Value,
        AccessoryId = line.AccessoryId?.Value,
        line.CostCode,
        line.Quantity,
        line.Unit,
        line.IsRequired,
        line.SortOrder
    };
}
