namespace Production.Application.Contracts;

/// <summary>What the person may do on «Склад тканин».</summary>
public sealed record FabricStockPermissions(bool CanMove, bool SeesValue);

/// <summary>
/// A line of «Склад тканин». <c>Id</c> is null for a fabric and colour open orders plan but the stock has never held,
/// so the shortage is visible. <c>Value</c> and <c>Price</c> are null for people who do not see the value.
/// </summary>
public sealed record FabricStockRowResponse(
    int? Id,
    int? FabricId,
    string FabricCode,
    string FabricName,
    string Color,
    decimal Meters,
    decimal Reserved,
    decimal Available,
    decimal? Price,
    decimal? Value,
    int Version,
    DateTimeOffset? UpdatedAt);

public sealed record FabricStockTotals(decimal Meters, decimal Reserved, int Lines, int Short, decimal? Value);

public sealed record FabricStockListResponse(
    IReadOnlyList<FabricStockRowResponse> Rows,
    FabricStockTotals Totals,
    FabricStockPermissions Permissions);

/// <param name="Kind">Opening, Receipt, Issue, Adjustment or Reversal.</param>
public sealed record FabricMovementResponse(
    int Id,
    string Kind,
    decimal MetersBefore,
    decimal Delta,
    decimal MetersAfter,
    DateOnly OperationDate,
    string? Note,
    int? SupplierId,
    string? SupplierName,
    string? Document,
    int? OrderId,
    int? OrderNumber,
    string? ModelName,
    int? ReversesMovementId,
    bool IsReversed,
    bool CanReverse,
    string CreatedByName,
    DateTimeOffset CreatedAt);

/// <summary>An item of a «Нове» order that holds part of this line.</summary>
public sealed record FabricReservationResponse(int OrderId, int OrderNumber, int ItemId, string ModelName,
    decimal Planned, decimal Covered, decimal Reserved);

public sealed record FabricStockLineResponse(
    FabricStockRowResponse Line,
    IReadOnlyList<FabricMovementResponse> Movements,
    IReadOnlyList<FabricReservationResponse> Reservations,
    FabricStockPermissions Permissions);

/// <summary>A fabric of «Тканини» with a FAB- code, to pick on the stock.</summary>
public sealed record FabricStockFabricChoice(int Id, string Code, string Name);

/// <summary>An active supplier of «Постачальники», to pick on a receipt.</summary>
public sealed record FabricStockSupplierChoice(int Id, string Name);

/// <summary>What the stock forms pick from, so the workshop does not need access to «Довідники».</summary>
public sealed record FabricStockChoicesResponse(
    IReadOnlyList<FabricStockFabricChoice> Fabrics,
    IReadOnlyList<FabricStockSupplierChoice> Suppliers);

/// <summary>«Початковий залишок» or «Прихід» of a fabric and colour; the stock line is created when it is new.</summary>
/// <param name="Kind">Opening (only the first movement of a line) or Receipt.</param>
/// <param name="Version">The line's version; null when the line is new.</param>
public sealed record CreateFabricMovementInput(
    string Kind,
    int FabricId,
    string Color,
    decimal Meters,
    DateOnly OperationDate,
    int? SupplierId,
    string? Document,
    string? Note,
    int? Version);

/// <summary>«Коригування»: what was counted on the shelf, with a reason.</summary>
public sealed record CorrectFabricStockInput(int Version, decimal CountedMeters, DateOnly OperationDate, string Note);

/// <summary>«Сторно» of an opening, receipt or correction, with a reason.</summary>
public sealed record ReverseFabricMovementInput(int Version, string Note);

/// <summary>A planned colour of an item and how far issues covered it.</summary>
/// <param name="StockId">The stock line of that fabric and colour; null when the stock never held it.</param>
public sealed record FabricPlanPartResponse(
    string FabricCode,
    string Color,
    string ColorKey,
    decimal Planned,
    decimal Covered,
    decimal Issued,
    decimal Reserved,
    int? StockId,
    decimal StockMeters,
    decimal StockAvailable);

/// <param name="Purpose">Planned, Replacement or Additional.</param>
public sealed record FabricIssueLineResponse(
    int Id,
    string Purpose,
    string? PlanColorKey,
    decimal? PlannedMeters,
    int StockId,
    string FabricCode,
    string FabricName,
    string Color,
    decimal Meters,
    decimal CoveredMeters);

public sealed record FabricIssueResponse(
    int Id,
    DateOnly IssuedOn,
    string? Note,
    string CreatedByName,
    DateTimeOffset CreatedAt,
    bool IsReversed,
    DateTimeOffset? ReversedAt,
    string? ReversedByName,
    string? ReverseReason,
    IReadOnlyList<FabricIssueLineResponse> Lines);

/// <summary>Tab «Тканина» of an item: plan by colour, issues, and whether the person may issue.</summary>
public sealed record ProductionItemFabricResponse(
    int OrderId,
    int ItemId,
    int OrderVersion,
    string? FabricCode,
    string? FabricName,
    int Quantity,
    bool HasNorm,
    IReadOnlyList<FabricPlanPartResponse> Plan,
    decimal Issued,
    IReadOnlyList<FabricIssueResponse> Issues,
    bool CanIssue);

/// <param name="Purpose">Planned, Replacement or Additional.</param>
/// <param name="PlanColorKey">The planned colour a planned or replacement line stands for; null for additional.</param>
/// <param name="FabricId">The fabric of «Тканини» that is really issued.</param>
public sealed record FabricIssueLineInput(string Purpose, string? PlanColorKey, int FabricId, string Color,
    decimal Meters);

public sealed record CreateFabricIssueInput(int OrderVersion, DateOnly IssuedOn, string? Note,
    IReadOnlyList<FabricIssueLineInput> Lines);

public sealed record ReverseFabricIssueInput(int OrderVersion, string Reason);

/// <summary>Movements of a fabric and colour over a period, for «Зведення витрати».</summary>
public sealed record FabricConsumptionRowResponse(
    int StockId,
    string FabricCode,
    string FabricName,
    string Color,
    decimal Received,
    decimal Issued,
    decimal Corrected,
    decimal? IssuedValue);

public sealed record FabricConsumptionResponse(
    DateOnly From,
    DateOnly To,
    IReadOnlyList<FabricConsumptionRowResponse> Rows,
    decimal Received,
    decimal Issued,
    decimal? IssuedValue);

/// <summary>A stock line of the old «Цех» with its movements in their order.</summary>
public sealed record FabricStockImportRow(
    string LegacyId,
    string FabricCode,
    string Color,
    decimal Meters,
    IReadOnlyList<FabricMovementImportRow> Movements);

/// <param name="Kind">Opening, Receipt, Issue, Adjustment or Reversal.</param>
/// <param name="OrderLegacyId">For an issue: the old order id, as in production-orders.json.</param>
/// <param name="Position">For an issue: the item's position in that order.</param>
public sealed record FabricMovementImportRow(
    string LegacyId,
    string Kind,
    decimal MetersBefore,
    decimal Delta,
    decimal MetersAfter,
    DateOnly OperationDate,
    DateTimeOffset At,
    string? Supplier,
    string? Document,
    string? Note,
    string? ActorEmail,
    string? OrderLegacyId,
    int? Position);

/// <summary>A «Видача» package of the old «Цех»; its lines point to the movements above.</summary>
public sealed record FabricIssueImportRow(
    string LegacyId,
    string OrderLegacyId,
    int Position,
    DateOnly IssuedOn,
    DateTimeOffset At,
    string? ActorEmail,
    string? Note,
    IReadOnlyList<FabricIssueLineImportRow> Lines);

/// <param name="Purpose">Planned, Replacement or Additional.</param>
public sealed record FabricIssueLineImportRow(
    string Purpose,
    string? PlanFabricCode,
    string? PlanColor,
    decimal? PlannedMeters,
    string MovementLegacyId,
    decimal CoveredMeters);

/// <summary>A reservation of the old app, only to compare with the recomputed one.</summary>
public sealed record FabricReservationImportRow(string FabricCode, string Color, decimal Meters);

public sealed record FabricStockImportInput(
    IReadOnlyList<FabricStockImportRow> Stocks,
    IReadOnlyList<FabricIssueImportRow> Issues,
    IReadOnlyList<FabricReservationImportRow> OldReservations);

/// <param name="Action">created, unchanged or error.</param>
public sealed record FabricStockImportLine(string FabricCode, string Color, int Movements, string Action,
    string? Error);

public sealed record FabricStockImportResult(
    bool DryRun,
    int Created,
    int Unchanged,
    int Errors,
    int Movements,
    int Issues,
    int ReservationDifferences,
    IReadOnlyList<FabricStockImportLine> Lines,
    IReadOnlyList<string> IssueErrors);
