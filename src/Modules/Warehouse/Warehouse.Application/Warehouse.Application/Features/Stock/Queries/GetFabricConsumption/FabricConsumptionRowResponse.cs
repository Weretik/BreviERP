namespace Warehouse.Application.Features.Stock.Queries.GetFabricConsumption;

/// <summary>Movements of a fabric and colour over a period, for «Витрата за період».</summary>
/// <param name="Issued">Everything issued: to tasks and one-off.</param>
/// <param name="IssuedToTasks">Issued to items of orders.</param>
/// <param name="OneOff">«Разова витрата»: issued without a task.</param>
public sealed record FabricConsumptionRowResponse(
    int StockId,
    string FabricCode,
    string FabricName,
    string Color,
    decimal Received,
    decimal Issued,
    decimal Corrected,
    decimal? IssuedValue,
    decimal IssuedToTasks = 0,
    decimal OneOff = 0);
