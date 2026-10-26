using Catalog.Domain.Products.Entities;

namespace SewingPricing.Jobs.Review;

internal static class OperationLineReviewComparer
{
    public static IReadOnlyList<PricingReviewCheck> Compare(SewingOperationLine expected, SewingOperationLine? actual) =>
        PricingReviewComparison.Fields(Fields(expected), actual is null ? null : Fields(actual));

    private static object Fields(SewingOperationLine line) => new
    {
        ProductId = line.ProductId.Value,
        ArticleId = line.ArticleId?.Value,
        OperationId = line.OperationId.Value,
        line.Quantity,
        line.ManualMinutes,
        line.IsRequired,
        line.SortOrder
    };
}
