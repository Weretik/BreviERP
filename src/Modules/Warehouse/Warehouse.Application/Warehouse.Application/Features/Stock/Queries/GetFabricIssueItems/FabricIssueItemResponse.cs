namespace Warehouse.Application.Features.Stock.Queries.GetFabricIssueItems;

/// <summary>An item of an open order that fabric may be issued to.</summary>
public sealed record FabricIssueItemResponse(
    int OrderId,
    int OrderNumber,
    string ClientName,
    int ItemId,
    string ModelName,
    string? FabricCode,
    string? FabricName,
    int Quantity);
