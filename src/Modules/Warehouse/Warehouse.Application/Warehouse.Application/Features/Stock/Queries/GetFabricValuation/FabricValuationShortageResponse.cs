namespace Warehouse.Application.Features.Stock.Queries.GetFabricValuation;

/// <param name="Meters">The shortage as a positive figure.</param>
public sealed record FabricValuationShortageResponse(
    string FabricCode,
    string FabricName,
    string Color,
    decimal Meters,
    decimal? Price,
    decimal? Value);
