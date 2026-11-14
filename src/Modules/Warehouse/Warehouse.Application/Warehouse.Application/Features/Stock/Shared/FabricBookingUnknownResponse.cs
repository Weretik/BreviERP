namespace Warehouse.Application.Features.Stock.Shared;

/// <summary>An item whose need is unknown: it is not counted as zero.</summary>
public sealed record FabricBookingUnknownResponse(
    int OrderId,
    int OrderNumber,
    string ClientName,
    int ItemId,
    string ModelName,
    string Reason);
