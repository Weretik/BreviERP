namespace Warehouse.Application.Features.Stock.Queries.GetFabricValuation.Results;

public sealed record FabricValuationColorResponse(string Color, decimal Meters, decimal? Price, decimal? Value);
