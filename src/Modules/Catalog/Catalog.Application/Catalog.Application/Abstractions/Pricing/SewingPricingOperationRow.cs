namespace Catalog.Application.Abstractions.Pricing;

public sealed record SewingPricingOperationRow(
    long LineId, int OperationId, decimal Quantity, decimal? ManualMinutes);

