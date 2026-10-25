using Catalog.Domain.Pricing.Sewing;

namespace Catalog.Application.Contracts.Pricing;

public sealed record SewingArticlePricePreview(
    int ProductId, long ArticleId, string SaleSku, long CompositionVersion,
    DateTimeOffset At, long? SettingsVersionId, IReadOnlyList<long> ReferenceVersionIds,
    SewingArticleCalculation Calculation,
    SewingArticlePricingSnapshot Inputs,
    SewingPricingSettingsSelection? Settings,
    IReadOnlyList<PricingReferenceValue> References);

