namespace Catalog.Application.Contracts.Pricing;

public sealed record SewingArticlePricingSnapshot(
    int ProductId, long ArticleId, string SaleSku, long CompositionVersion, bool IsActive,
    IReadOnlyList<SewingPricingMaterialRow> Materials, IReadOnlyList<SewingPricingOperationRow> Operations);

