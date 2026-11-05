namespace Production.Application.Contracts;

/// <summary>«Звіти цеху» (PRD-D48): the chosen months and, when asked, the months to compare with.</summary>
public sealed record WorkshopReportPairResponse(WorkshopReportResponse Current, WorkshopReportResponse? Comparison);

/// <summary>
/// The report of the months <c>From</c>..<c>To</c> («РРРР-ММ»). <c>ClosedMonths</c> are read as they were closed in
/// «Зарплата»; the others are counted from the current data and can still change.
/// </summary>
public sealed record WorkshopReportResponse(
    string From,
    string To,
    IReadOnlyList<string> ClosedMonths,
    IReadOnlyList<string> OpenMonths,
    WorkshopReportTotalsResponse Totals,
    IReadOnlyList<ReportOutputRowResponse> Output,
    IReadOnlyList<ReportStaffRowResponse> Staff,
    IReadOnlyList<ReportIssueRowResponse> Materials,
    IReadOnlyList<ReportStockRowResponse> Stock,
    IReadOnlyList<string> Warnings);

/// <summary>
/// Money counts only what has a price: pieces without the КП price and metres without a fabric price are given apart.
/// </summary>
public sealed record WorkshopReportTotalsResponse(
    int SewnPieces,
    decimal OutputValue,
    int UnpricedPieces,
    decimal Accrued,
    int Minutes,
    decimal IssuedMeters,
    decimal IssuedValue,
    decimal UnpricedIssuedMeters,
    decimal StockMeters,
    decimal StockValue,
    decimal UnpricedStockMeters);

/// <summary>Pieces sewn of an item in the months (within the plan, not cancelled) at the item's КП price.</summary>
public sealed record ReportOutputRowResponse(int OrderId, int OrderNumber, string ClientName, int ItemId,
    string ModelName, int Quantity, decimal? UnitPrice, decimal? Value);

/// <summary>A worker over the months; <c>DayAverage</c> is the pay per 8 hours of «Робочий час», null without hours.</summary>
public sealed record ReportStaffRowResponse(int EmployeeId, string Name, string Position, int Minutes, int Sewn,
    int Cut, decimal Accrued, decimal? DayAverage);

/// <summary>Fabric issued to an order item in the months (issues not reversed), at the fabric's price.</summary>
public sealed record ReportIssueRowResponse(int MovementId, DateOnly IssuedOn, string FabricCode, string FabricName,
    string Color, int? OrderId, int? OrderNumber, string? ModelName, decimal Meters, decimal? PricePerMeter,
    decimal? Value);

/// <summary>The balance of a fabric and colour at the end of the last month, at the fabric's current price.</summary>
public sealed record ReportStockRowResponse(int StockId, string FabricCode, string FabricName, string Color,
    decimal Meters, decimal? PricePerMeter, decimal? Value);

/// <summary>
/// «Огляд виробництва» on the admin home (PRD-D50): open orders by stage, late ones and what is ready to ship.
/// <c>Hours</c> is the hours of this week by worker, only for those who keep «Робочий час».
/// </summary>
public sealed record WorkshopDashboardResponse(
    DateOnly Today,
    int OpenOrders,
    int OpenPieces,
    int LateOrders,
    int ReadyOrders,
    int ReadyPieces,
    IReadOnlyList<DashboardStageResponse> Stages,
    IReadOnlyList<DashboardHoursResponse>? Hours);

/// <summary>A column of the board: the orders whose slowest item is at this stage.</summary>
public sealed record DashboardStageResponse(int Stage, string Label, IReadOnlyList<DashboardOrderResponse> Orders);

/// <summary>An open order on the board; <c>Progress</c> is the share of pieces sewn within the plan, 0–100.</summary>
public sealed record DashboardOrderResponse(int Id, int Number, string ClientName, string Product, int Items,
    int Quantity, int Progress, DateOnly DueDate, bool IsLate);

public sealed record DashboardHoursResponse(int EmployeeId, string Name, int Minutes);
