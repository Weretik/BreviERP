namespace Production.Application.Features.Orders.Queries.GetProductionQuoteDraft;

/// <param name="SewingRate">Null for a manager.</param>
public sealed record ProductionQuoteDraftOption(
    long OptionId, string Title, string? FabricCode, string? FabricName, decimal? Retail, decimal? Wholesale,
    decimal? Large, decimal? SewingRate);
