namespace Catalog.Application.Abstractions.Pricing.Inputs;

public sealed record SewingPricingOperationRow(
    long LineId, int OperationId, decimal Quantity, decimal? ManualMinutes);

