namespace Catalog.Application.Contracts.Pricing;

public sealed record SewingPricingMaterialRow(
    long LineId, string ReferenceKey, decimal Quantity, string Unit, bool IsFabric, bool IsOtherCost,
    bool IsRequired, bool IsReviewed);

