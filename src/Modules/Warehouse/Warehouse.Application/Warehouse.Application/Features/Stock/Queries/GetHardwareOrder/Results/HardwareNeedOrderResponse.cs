namespace Warehouse.Application.Features.Stock.Queries.GetHardwareOrder.Results;

/// <summary>One item's share: norm × products = quantity.</summary>
public sealed record HardwareNeedOrderResponse(
    int OrderId,
    int OrderNumber,
    string ClientName,
    int ItemId,
    string ModelName,
    decimal Norm,
    int Products,
    decimal Quantity,
    DateOnly? DueDate = null);
