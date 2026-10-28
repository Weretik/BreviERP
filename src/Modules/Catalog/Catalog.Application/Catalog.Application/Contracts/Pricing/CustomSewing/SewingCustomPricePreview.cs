using Catalog.Domain.Pricing.Sewing;

namespace Catalog.Application.Contracts.Pricing;

public sealed record SewingCustomPricePreview(
    DateTimeOffset EvaluatedAt, SewingCustomPricingInput Input,
    SewingPricingSettingsSelection? Settings, IReadOnlyList<PricingReferenceValue> ReferenceVersions,
    SewingCustomCalculation Calculation);
