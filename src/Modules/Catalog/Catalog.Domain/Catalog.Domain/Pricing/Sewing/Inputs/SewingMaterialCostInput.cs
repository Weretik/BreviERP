namespace Catalog.Domain.Pricing.Sewing;

public sealed record SewingMaterialCostInput(
    string LineKey, decimal Quantity, decimal? UnitCost, bool IsOtherCost,
    bool IsFabric, bool IsRequired, bool IsReviewed, bool IsActive, bool IsApproved, bool UnitMatches);

