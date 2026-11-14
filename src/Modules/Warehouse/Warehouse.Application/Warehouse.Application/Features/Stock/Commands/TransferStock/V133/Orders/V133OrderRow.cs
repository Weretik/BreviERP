namespace Warehouse.Application.Features.Stock.Commands.TransferStock.V133.Orders;

/// <summary><c>production_orders</c>: only what links the stock to the orders.</summary>
public sealed record V133OrderRow(string Id, long Seq, string? Status, string? DeletedAt);
