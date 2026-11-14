using Catalog.Application.Abstractions.Pricing.Inputs;

namespace Catalog.Application.Abstractions.Pricing.Calculations;

public sealed record SewingArticlePricingSnapshot(
    int ProductId, long ArticleId, string SaleSku, long CompositionVersion, bool IsActive,
    IReadOnlyList<SewingPricingMaterialRow> Materials, IReadOnlyList<SewingPricingOperationRow> Operations);

