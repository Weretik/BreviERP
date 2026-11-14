namespace Warehouse.Application.Features.Stock.Queries.GetFabricValuation;

/// <param name="Price">UAH per metre; null when the group has no positive price.</param>
/// <param name="PriceFabricCode">The offer the price was taken from.</param>
public sealed record FabricValuationFabricResponse(
    string FabricCode,
    string FabricName,
    decimal? Price,
    string? PriceFabricCode,
    decimal Meters,
    decimal UnpricedMeters,
    decimal Value,
    IReadOnlyList<FabricValuationColorResponse> Colors);
