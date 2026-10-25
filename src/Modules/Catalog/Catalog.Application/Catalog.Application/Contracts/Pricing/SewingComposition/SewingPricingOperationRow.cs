namespace Catalog.Application.Contracts.Pricing;

public sealed record SewingPricingOperationRow(
    long LineId, int OperationId, decimal Quantity, decimal? ManualMinutes, bool IsReviewed);

