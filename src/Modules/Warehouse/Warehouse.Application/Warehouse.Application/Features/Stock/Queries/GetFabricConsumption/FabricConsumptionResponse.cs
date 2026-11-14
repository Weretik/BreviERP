namespace Warehouse.Application.Features.Stock.Queries.GetFabricConsumption;

public sealed record FabricConsumptionResponse(
    DateOnly From,
    DateOnly To,
    IReadOnlyList<FabricConsumptionRowResponse> Rows,
    decimal Received,
    decimal Issued,
    decimal? IssuedValue,
    decimal IssuedToTasks = 0,
    decimal OneOff = 0,
    decimal Corrected = 0);
