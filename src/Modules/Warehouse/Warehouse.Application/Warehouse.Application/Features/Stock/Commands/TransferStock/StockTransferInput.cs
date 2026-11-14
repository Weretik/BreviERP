using Warehouse.Application.Features.Stock.Commands.TransferStock.V133.Orders;
using Warehouse.Application.Features.Stock.Commands.TransferStock.V133.Stock;

namespace Warehouse.Application.Features.Stock.Commands.TransferStock;

// «Перенесення з v133» (WH-006): rows of the D1 tables of the old «Цех», one JSONL line each, with the column names in
// camelCase and the values as they are. Hundredths of a metre stay whole numbers; dates and times stay text.

/// <summary>The tables of a D1 export the transfer reads; the other tables are not needed.</summary>
/// <param name="Snapshot">What the export is, as the person names it (for the journal).</param>
public sealed record StockTransferInput(
    string? Snapshot,
    IReadOnlyList<V133StockRow> FabricStock,
    IReadOnlyList<V133MovementRow> FabricStockMovements,
    IReadOnlyList<V133RevisionRow> FabricMovementRevisions,
    IReadOnlyList<V133BatchRow> FabricIssueBatches,
    IReadOnlyList<V133IssueLineRow> FabricIssueLines,
    IReadOnlyList<V133IssueComponentRow> FabricIssueComponents,
    IReadOnlyList<V133ReservationRow> FabricReservations,
    IReadOnlyList<V133OrderRow> ProductionOrders,
    IReadOnlyList<V133ItemRow> ProductionItems);
