namespace Warehouse.Application.Contracts;

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

/// <summary><c>fabric_stock</c>: a stock line of a fabric (FAB- code) in a colour.</summary>
public sealed record V133StockRow(
    string Id,
    string FabricId,
    string? FabricName,
    string Color,
    string? NormalizedColor,
    long HundredthsOfMeter,
    string? LastMovementId,
    string? UpdatedAt);

/// <summary><c>fabric_stock_movements</c>: kind opening, receipt, issue, adjustment or void (a voided movement).</summary>
public sealed record V133MovementRow(
    string Id,
    string StockId,
    string? FabricId,
    string? FabricName,
    string? Color,
    string Kind,
    long BeforeHundredths,
    long DeltaHundredths,
    long AfterHundredths,
    string? OrderId,
    string? ItemId,
    string? OrderNumber,
    string? ModelName,
    string? ClientName,
    long? ItemQuantity,
    string? Supplier,
    string? Reference,
    string? Note,
    string? ActorEmail,
    string At,
    string? OperationDate);

/// <summary><c>fabric_movement_revisions</c>: an edit or void of a movement with its state before and after (JSON).</summary>
public sealed record V133RevisionRow(
    string Id,
    string? OperationId,
    string MovementId,
    string BeforeJson,
    string AfterJson,
    string? ActorEmail,
    string At);

/// <summary><c>fabric_issue_batches</c>: one «Видача» package of an item.</summary>
/// <param name="ResultJson">Only its beginning is read, for the number of products cut.</param>
public sealed record V133BatchRow(
    string Id,
    string ItemId,
    string? ActorEmail,
    string? ResultJson,
    string CreatedAt);

/// <summary><c>fabric_issue_lines</c>: a line of a package; purpose planned, replacement or additional.</summary>
public sealed record V133IssueLineRow(
    string Id,
    string BatchId,
    string ItemId,
    string StockId,
    string Purpose,
    string? PlanKey,
    string? PlannedFabricId,
    string? PlannedColor,
    long? PlannedHundredths,
    string? ActualFabricId,
    string? ActualColor,
    long ActualHundredths,
    long CoveredPlanHundredths,
    string? Note,
    string CreatedAt,
    string? PlannedComponentJson,
    string? VoidedAt);

/// <summary><c>fabric_issue_components</c>: the movement a line was issued with.</summary>
public sealed record V133IssueComponentRow(string MovementId, string LineId);

/// <summary><c>fabric_reservations</c>: the saved main-fabric plan of an item.</summary>
public sealed record V133ReservationRow(
    string Id,
    string? OrderId,
    string ItemId,
    string FabricId,
    string Color,
    long HundredthsOfMeter);

/// <summary><c>production_orders</c>: only what links the stock to the orders.</summary>
public sealed record V133OrderRow(string Id, long Seq, string? Status, string? DeletedAt);

/// <summary><c>production_items</c>: only what links the stock to the items; their order in the file is their position.</summary>
public sealed record V133ItemRow(string Id, string OrderId, string? ModelArticle, string? ModelName, long Quantity);

/// <summary>What the transfer did, or would do on a dry run.</summary>
/// <param name="Applied">The Brevi stock was replaced.</param>
/// <param name="Unchanged">This export was already transferred and nothing was entered in Brevi since.</param>
/// <param name="Hash">SHA-256 of the export rows; the journal knows each transferred export by it.</param>
/// <param name="Target">What Brevi gets; null when the export has errors or is unchanged.</param>
/// <param name="Current">What the Brevi stock holds now, and will lose.</param>
/// <param name="NeedsConfirmation">Records entered in Brevi itself would be removed; they are listed in Current.</param>
public sealed record StockTransferResult(
    bool Applied,
    bool Unchanged,
    string Hash,
    StockTransferSource Source,
    StockTransferTarget? Target,
    StockTransferCurrent Current,
    bool NeedsConfirmation,
    IReadOnlyList<StockTransferProblem> Errors,
    IReadOnlyList<StockTransferProblem> Notes,
    DateTimeOffset? LastTransferAt);

/// <summary>Rows of the export and its control numbers.</summary>
public sealed record StockTransferSource(
    int Lines,
    int Pairs,
    int Movements,
    int Openings,
    int Receipts,
    int Issues,
    int Adjustments,
    int Voided,
    int Revisions,
    int Packages,
    int IssueLines,
    int Reservations,
    decimal Meters,
    int NegativePairs,
    int ZeroPairs);

/// <summary>What Brevi gets.</summary>
public sealed record StockTransferTarget(
    int Lines,
    int Movements,
    int Revisions,
    int Issues,
    int IssueLines,
    int Plans,
    decimal Meters);

/// <summary>The Brevi stock before the transfer; <c>Own…</c> were entered in Brevi, not moved from the old «Цех».</summary>
public sealed record StockTransferCurrent(
    int Lines,
    int Movements,
    int Issues,
    int Plans,
    int OwnLines,
    int OwnMovements,
    int OwnIssues);

/// <summary>A row the transfer refuses (error) or treats in its own way (note).</summary>
public sealed record StockTransferProblem(string Table, string? RowId, string Message);
