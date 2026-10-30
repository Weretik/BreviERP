using Catalog.Domain.Pricing.Sewing;

namespace Catalog.Application.Contracts.Pricing;

public sealed record SewingCustomPricingInput(
    IReadOnlyList<SewingPricingMaterialRow> Materials,
    IReadOnlyList<SewingPricingOperationRow> Operations, SewingCustomCharges Charges);
