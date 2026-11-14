namespace Warehouse.Application.Features.Stock.Queries.GetFabricValuation;

public sealed record FabricValuationColorResponse(string Color, decimal Meters, decimal? Price, decimal? Value);
