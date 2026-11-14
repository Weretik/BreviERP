namespace Warehouse.Application.Features.Stock.Commands.TransferStock.V133.Stock;

/// <summary><c>fabric_issue_batches</c>: one «Видача» package of an item.</summary>
/// <param name="ResultJson">Only its beginning is read, for the number of products cut.</param>
public sealed record V133BatchRow(
    string Id,
    string ItemId,
    string? ActorEmail,
    string? ResultJson,
    string CreatedAt);
