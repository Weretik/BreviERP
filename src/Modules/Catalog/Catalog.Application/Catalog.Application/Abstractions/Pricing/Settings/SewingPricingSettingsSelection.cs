using Catalog.Domain.Pricing.Sewing;

namespace Catalog.Application.Abstractions.Pricing.Settings;

public sealed record SewingPricingSettingsSelection(
    long Id, int Revision, DateTimeOffset EffectiveFrom, string FormulaVersion, SewingCalculationSettings Settings);

