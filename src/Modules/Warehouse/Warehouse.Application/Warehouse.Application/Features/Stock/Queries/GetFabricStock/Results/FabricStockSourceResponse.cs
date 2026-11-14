namespace Warehouse.Application.Features.Stock.Queries.GetFabricStock.Results;

/// <summary>A stock line inside a pair: its own code and balance.</summary>
public sealed record FabricStockSourceResponse(int Id, string FabricCode, decimal Meters);
