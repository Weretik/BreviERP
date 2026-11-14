namespace Warehouse.Application.Features.Stock.Commands.TransferStock;

/// <summary><c>production_items</c>: only what links the stock to the items; their order in the file is their position.</summary>
public sealed record V133ItemRow(string Id, string OrderId, string? ModelArticle, string? ModelName, long Quantity);
