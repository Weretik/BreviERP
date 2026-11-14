namespace Warehouse.Application.Features.Stock.Queries.GetHardwareOrder;

public sealed record HardwareIncompleteResponse(
    int OrderId,
    int OrderNumber,
    string ClientName,
    int ItemId,
    string ModelName,
    string? Code,
    string? Name,
    string Reason);
