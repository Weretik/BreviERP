namespace Catalog.Domain.Pricing.Sewing;

public sealed record SewingOperationCostInput(
    string LineKey, decimal Quantity, decimal? StandardMinutes, decimal? ManualMinutes,
    bool IsReviewed, bool IsActive, bool IsApproved);

