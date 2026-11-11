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
    bool CanEdit,
    decimal? PerProduct = null);

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

/// <summary>A planned part of an item and how far issues covered it.</summary>
/// <param name="FabricCode">The planned fabric as in the plan; the pair is its alias group.</param>
/// <param name="Layer">Upper, Lining, Insulation or Fleece.</param>
/// <param name="StockId">The first stock line of that pair; null when the stock never held it.</param>
/// <param name="StockMeters">Metres of the pair (every line of the alias group in that colour).</param>
/// <param name="StockAvailable">The pair minus what open orders hold of it; informational.</param>
/// <param name="Missing">What is still to be cut: the plan minus what issues covered.</param>
public sealed record FabricPlanPartResponse(
    string FabricCode,
    string Color,
    string ColorKey,
    string Layer,
    decimal Planned,
    decimal Covered,
    decimal Issued,
    decimal Reserved,
    int? StockId,
    decimal StockMeters,
    decimal StockAvailable,
    decimal Missing);

/// <param name="Purpose">Planned, Replacement or Additional.</param>
/// <param name="PlannedMeters">The plan of the products cut for its planned part (or the planned part, older issues).</param>
/// <param name="Layer">Layer of the planned part or role of an additional line; null when unknown.</param>
public sealed record FabricIssueLineResponse(
    int Id,
    string Purpose,
    string? PlanFabricCode,
    string? PlanColorKey,
    decimal? PlannedMeters,
    int StockId,
    string FabricCode,
    string FabricName,
    string Color,
    decimal Meters,
    decimal CoveredMeters,
    string? Layer,
    string? Note,
    bool IsVoided);

/// <summary>Plan and fact of one planned part in an issue by cutting (TZ §7.2).</summary>
/// <param name="Plan">P × n / N, half up to hundredths; counted once for every line of the part.</param>
/// <param name="Difference">Fact minus plan; positive is overspending.</param>
/// <param name="PerProduct">Fact per product cut.</param>
public sealed record FabricIssueComparisonResponse(
    string PlanFabricCode,
    string PlanColorKey,
    string Color,
    decimal Plan,
    decimal Actual,
    decimal Difference,
    decimal PerProduct);

/// <param name="IssuedQuantity">Products cut; null for issues before issues by cutting.</param>
/// <param name="Comparison">Plan and fact by planned part; empty for older issues.</param>
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
    int? IssuedQuantity,
    IReadOnlyList<FabricIssueComparisonResponse> Comparison,
    IReadOnlyList<FabricIssueLineResponse> Lines);

/// <summary>Tab «Тканина» of an item: plan by part, issues, and whether the person may issue.</summary>
/// <param name="PlanVersion">Version of the saved plan; 0 while the plan follows today's norm (no issue yet).</param>
/// <param name="PlanMissing">Why there is no plan; additional fabric may still be issued.</param>
/// <param name="CanIssue">The person may move stock and the order is open.</param>
public sealed record ProductionItemFabricResponse(
    int OrderId,
    int ItemId,
    int OrderNumber,
    string ClientName,
    string ModelName,
    int OrderVersion,
    bool IsOpen,
    string? FabricCode,
    string? FabricName,
    int Quantity,
    bool HasNorm,
    int PlanVersion,
    string? PlanMissing,
    IReadOnlyList<FabricPlanPartResponse> Plan,
    decimal Issued,
    IReadOnlyList<FabricIssueResponse> Issues,
    bool CanIssue);

/// <summary>A line of an issue by cutting: real metres of one pair of the stock.</summary>
/// <param name="FabricCode">Any code of the fabric's alias group, as «Залишки» show it.</param>
/// <param name="ExpectedVersion">The pair version the form showed; null skips the check.</param>
/// <param name="PlanFabricCode">With <paramref name="PlanColorKey"/>: the planned part this line is for; null for additional.</param>
/// <param name="Layer">Role of an additional line: Upper, Lining, Insulation or Fleece; null when unknown.</param>
public sealed record FabricIssueLineInput(
    string FabricCode,
    string Color,
    string? ExpectedVersion,
    decimal Meters,
    string? PlanFabricCode,
    string? PlanColorKey,
    string? Layer,
    string? Note);

/// <summary>«Видача» by actual cutting: products cut now and the real fabric (TZ §7).</summary>
/// <param name="PlanVersion">The plan version the form showed (0 before the first issue).</param>
public sealed record CreateFabricIssueInput(
    Guid OperationId,
    int OrderVersion,
    int PlanVersion,
    DateOnly IssuedOn,
    int IssuedQuantity,
    string? Note,
    IReadOnlyList<FabricIssueLineInput> Lines);

/// <summary>«Оновити план»: the norm of today replaces the saved plan of the item.</summary>
public sealed record RefreshItemFabricPlanInput(int OrderVersion, int PlanVersion);

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

public sealed record FabricIssueItemsResponse(IReadOnlyList<FabricIssueItemResponse> Items, bool HasMore);

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
