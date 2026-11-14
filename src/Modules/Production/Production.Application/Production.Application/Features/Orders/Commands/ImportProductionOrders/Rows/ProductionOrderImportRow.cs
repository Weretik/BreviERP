using Production.Application.Features.Orders.Shared.Validation;
namespace Production.Application.Features.Orders.Commands.ImportProductionOrders.Rows;

/// <summary>An order of the old «Цех» from the transfer file.</summary>
/// <param name="Status">New, Closed or Cancelled.</param>
public sealed record ProductionOrderImportRow(
    string LegacyId,
    int Number,
    string? QuoteNumber,
    string ClientName,
    string? ManagerEmail,
    string ManagerName,
    string Status,
    DateOnly OrderDate,
    string? Comment,
    IReadOnlyList<ProductionItemImportRow> Items);
