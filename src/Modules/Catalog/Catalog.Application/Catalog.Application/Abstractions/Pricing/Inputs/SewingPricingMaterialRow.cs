namespace Catalog.Application.Abstractions.Pricing.Inputs;

public sealed record SewingPricingMaterialRow(
    long LineId, string ReferenceKey, decimal Quantity, string Unit, bool IsFabric, bool IsOtherCost);

