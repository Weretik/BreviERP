namespace Warehouse.Application.Features.Stock.Queries.GetFabricStock;

/// <summary>A task that needs a pair: what is left of its plan and what of it the stock does not cover.</summary>
public sealed record FabricStockBookingResponse(
    int OrderId,
    int OrderNumber,
    string ClientName,
    string ModelName,
    DateOnly? DueDate,
    decimal Need,
    decimal ToOrder);
