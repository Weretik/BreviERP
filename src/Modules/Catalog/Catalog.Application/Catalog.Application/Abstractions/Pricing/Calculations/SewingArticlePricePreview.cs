using Catalog.Domain.Pricing.Sewing;
using Catalog.Application.Abstractions.Pricing.Settings;

namespace Catalog.Application.Abstractions.Pricing.Calculations;

public sealed record SewingArticlePricePreview(
    int ProductId, long ArticleId, string SaleSku, long CompositionVersion,
    DateTimeOffset At, long? SettingsVersionId, IReadOnlyList<long> ReferenceVersionIds,
    SewingArticleCalculation Calculation,
    SewingArticlePricingSnapshot Inputs,
    SewingPricingSettingsSelection? Settings,
    IReadOnlyList<PricingReferenceValue> References);

