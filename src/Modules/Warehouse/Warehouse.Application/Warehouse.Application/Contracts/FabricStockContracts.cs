namespace Warehouse.Application.Contracts;

/// <summary>What the person may do on «Склад тканин».</summary>
/// <param name="CanMove">Record openings, receipts, issues and «Уточнити».</param>
/// <param name="SeesValue">Prices of «Тканини» on the forms.</param>
public sealed record FabricStockPermissions(bool CanMove, bool SeesValue);

/// <summary>A stock line inside a pair: its own code and balance.</summary>
public sealed record FabricStockSourceResponse(int Id, string FabricCode, decimal Meters);

/// <summary>
/// A fabric and colour on the shelf: every line of the alias group with that colour summed. <c>Version</c> goes back
/// with «Уточнити» and a «Разова витрата».
/// </summary>
public sealed record FabricStockColorResponse(
    string Color,
    string ColorKey,
    decimal Meters,
    string Version,
    IReadOnlyList<FabricStockSourceResponse> Sources);

/// <summary>A fabric of «Залишки» by its canonical code with the colours the stock holds.</summary>
/// <param name="Codes">The codes of «Тканини» that are this fabric, the canonical first.</param>
public sealed record FabricStockFabricResponse(
    string FabricCode,
    IReadOnlyList<string> Codes,
    string FabricName,
    string? Composition,
    string? Density,
    decimal Meters,
    IReadOnlyList<FabricStockColorResponse> Colors);

/// <summary>«Залишки»: only fabrics and colours that have stock lines, zero and negative included.</summary>
public sealed record FabricStockListResponse(
    IReadOnlyList<FabricStockFabricResponse> Fabrics,
    decimal Meters,
    int Pairs,
    int Negative,
    FabricStockPermissions Permissions);

/// <summary>A row of «Рух тканини».</summary>
/// <param name="Kind">Opening, Receipt, Issue, Adjustment or Reversal.</param>
/// <param name="FabricCode">The code of the line moved; <c>GroupCode</c> is the fabric the stock shows.</param>
/// <param name="MetersAfter">Balance of that line after the movement; the pair total is on «Залишки».</param>
public sealed record FabricMovementRowResponse(
    int Id,
    int StockId,
    string Kind,
    DateOnly OperationDate,
    DateTimeOffset CreatedAt,
    string FabricCode,
    string GroupCode,
    string FabricName,
    string Color,
    string? SupplierName,
    string? Document,
    int? OrderId,
    int? ItemId,
    int? OrderNumber,
    string? ModelName,
    string? Destination,
    decimal Delta,
    decimal MetersAfter,
    string? Note,
    string CreatedByName,
    bool IsReversed,
    int? ReversesMovementId,
    bool CanReverse,
    bool IsVoided,
    int Revision,
    bool CanEdit);

/// <summary>A page of 50 movements, newest operation date first.</summary>
public sealed record FabricMovementPageResponse(
    IReadOnlyList<FabricMovementRowResponse> Rows,
    int Page,
    bool HasMore,
    FabricStockPermissions Permissions);

/// <summary>An offer of «Тканини» with a FAB- code to pick on the stock forms.</summary>
/// <param name="GroupCode">The canonical code: offers with the same one are the same fabric on the shelf.</param>
/// <param name="Price">UAH per metre; null for people who do not see prices.</param>
public sealed record FabricStockFabricChoice(
    int Id,
    string Code,
    string GroupCode,
    string Name,
    string? Composition,
    string? Density,
    string? Supplier,
    decimal? Price);

/// <summary>What the stock forms pick from, so the workshop does not need access to «Довідники».</summary>
/// <param name="Suppliers">Names of active «Постачальники» and of the suppliers of «Тканини»; the form may type another.</param>
/// <param name="Colors">Active colours of «Кольори» in list order.</param>
public sealed record FabricStockChoicesResponse(
    IReadOnlyList<FabricStockFabricChoice> Fabrics,
    IReadOnlyList<string> Suppliers,
    IReadOnlyList<string> Colors);

/// <summary>«Початковий залишок»: once per fabric and colour, zero allowed.</summary>
/// <param name="OperationId">A new id per form; the same id repeats the saved result.</param>
public sealed record OpenFabricStockInput(
    Guid OperationId,
    int FabricId,
    string Color,
    decimal Meters,
    DateOnly OperationDate);

/// <summary>«Прихід» of the offer picked.</summary>
public sealed record ReceiveFabricInput(
    Guid OperationId,
    int FabricId,
    string Color,
    decimal Meters,
    DateOnly OperationDate,
    string? SupplierName,
    string? Document,
    string? Note);

/// <summary>«Разова витрата / без замовлення» of a pair; the balance may go below zero.</summary>
/// <param name="ExpectedVersion">The pair version shown on the form; null skips the check.</param>
public sealed record IssueFabricInput(
    Guid OperationId,
    string FabricCode,
    string Color,
    string? ExpectedVersion,
    decimal Meters,
    DateOnly OperationDate,
    string? Destination,
    string? Note);

/// <summary>«Уточнити»: the counted total of a pair with a reason.</summary>
public sealed record AdjustFabricStockInput(
    Guid OperationId,
    string FabricCode,
    string Color,
    string ExpectedVersion,
    decimal TargetMeters,
    DateOnly OperationDate,
    string Reason);

/// <summary>«Сторно» of an opening, receipt or correction, with a reason.</summary>
public sealed record ReverseFabricMovementInput(string Reason);

/// <summary>
/// An edit of one movement. <paramref name="Meters"/> is the amount: positive for a receipt and an issue, from zero
/// for an opening, the signed non-zero change for an adjustment. A receipt may move to another offer
/// (<paramref name="FabricId"/>) and colour; an issue to another pair the stock holds (<paramref name="FabricCode"/>
/// and <paramref name="Color"/>). Supplier and document belong to a receipt, destination to an issue.
/// </summary>
/// <param name="ExpectedRevision">The movement's revision shown on the form.</param>
public sealed record EditFabricMovementInput(
    Guid OperationId,
    int ExpectedRevision,
    DateOnly OperationDate,
    decimal Meters,
    int? FabricId,
    string? FabricCode,
    string? Color,
    string? SupplierName,
    string? Document,
    string? Destination,
    string? Reason);

/// <summary>«Скасувати»: the movement stays in the history with no effect, with a reason.</summary>
public sealed record VoidFabricMovementInput(Guid OperationId, int ExpectedRevision, string Reason);

/// <summary>«Скасувати останню правку»: the state before the last edit or void comes back.</summary>
public sealed record RestoreFabricMovementInput(Guid OperationId, int ExpectedRevision);

/// <summary>The parts of a movement before or after a revision.</summary>
public sealed record FabricMovementStateResponse(
    string FabricCode,
    string Color,
    DateOnly OperationDate,
    decimal Delta,
    string? SupplierName,
    string? Document,
    string? Destination,
    bool IsVoided);

/// <param name="Action">Edit, Void or Restore.</param>
/// <param name="IsUndone">Taken back by a later restore.</param>
public sealed record FabricMovementRevisionResponse(
    int Id,
    int Number,
    string Action,
    FabricMovementStateResponse Before,
    FabricMovementStateResponse After,
    string? Reason,
    string CreatedByName,
    DateTimeOffset CreatedAt,
    bool IsUndone);

/// <summary>«Історія правок» of a movement, newest first.</summary>
/// <param name="CanRestore">The last edit or void can be taken back now.</param>
public sealed record FabricMovementHistoryResponse(
    int MovementId,
    int Revision,
    bool CanRestore,
    IReadOnlyList<FabricMovementRevisionResponse> Revisions);

/// <summary>The pair after a stock operation.</summary>
/// <param name="Replayed">True when the same operation id was saved before and nothing moved now.</param>
public sealed record FabricStockOperationResponse(
    Guid? OperationId,
    string FabricCode,
    string Color,
    decimal Meters,
    bool Replayed);

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
