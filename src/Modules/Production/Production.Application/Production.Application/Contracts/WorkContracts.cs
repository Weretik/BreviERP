namespace Production.Application.Contracts;

/// <summary>A record of «Журнал робіт».</summary>
/// <param name="Kind">Sewing or Cutting.</param>
/// <param name="Rate">The item's sewing rate in UAH when the work was recorded.</param>
public sealed record ProductionWorkResponse(
    int Id,
    int OrderId,
    int OrderNumber,
    string ClientName,
    int ItemId,
    string ModelName,
    int EmployeeId,
    string EmployeeName,
    string Kind,
    int Quantity,
    DateOnly WorkDate,
    bool OverPlan,
    string? Reason,
    string? Comment,
    decimal Rate,
    bool IsVoided,
    DateTimeOffset? VoidedAt,
    string? VoidedByName,
    string? VoidReason,
    string CreatedByName,
    DateTimeOffset CreatedAt);

/// <summary>An active worker of «Працівники цеху» to pick on a record.</summary>
public sealed record WorkEmployeeChoice(int Id, string FullName, string Position);

/// <summary>An item of a «Нове» order that work can be recorded for.</summary>
public sealed record WorkItemChoice(int OrderId, int OrderNumber, string ClientName, int ItemId, string ModelName,
    int Quantity, int Sewn, int Cut, decimal? SewingRate);

/// <summary>What the forms of «Журнал робіт» pick from.</summary>
public sealed record WorkChoicesResponse(
    IReadOnlyList<WorkEmployeeChoice> Employees,
    IReadOnlyList<WorkItemChoice> Items,
    IReadOnlyList<string> OverPlanReasons);

public sealed record WorkLogTotals(int Sewn, int Cut, int Records);

/// <summary>«Журнал робіт» of a month, newest first.</summary>
public sealed record WorkLogResponse(string Month, IReadOnlyList<ProductionWorkResponse> Rows, WorkLogTotals Totals);

/// <summary>«Робота» of an item: how much is sewn and cut of the plan, and every record.</summary>
/// <param name="CannotRecord">Why work cannot be recorded now, or null when it can.</param>
public sealed record ProductionItemWorkResponse(
    int OrderId,
    int ItemId,
    int OrderNumber,
    string ModelName,
    int Quantity,
    decimal? SewingRate,
    int Sewn,
    int Cut,
    IReadOnlyList<ProductionWorkResponse> Rows,
    bool CanRecord,
    string? CannotRecord);

/// <param name="Kind">Sewing or Cutting.</param>
/// <param name="Reason">One of the over-plan reasons; required when <paramref name="OverPlan"/> is set.</param>
public sealed record CreateProductionWorkInput(int EmployeeId, string Kind, int Quantity, DateOnly WorkDate,
    bool OverPlan, string? Reason, string? Comment);

public sealed record VoidProductionWorkInput(string Reason);

/// <summary>A record of the old «Цех» work log.</summary>
/// <param name="OrderLegacyId">The old order id, as in production-orders.json.</param>
/// <param name="Position">The item's position in that order.</param>
public sealed record ProductionWorkImportRow(
    string LegacyId,
    string OrderLegacyId,
    int Position,
    string EmployeeName,
    string Kind,
    int Quantity,
    bool OverPlan,
    string? Reason,
    string? Comment,
    decimal Rate,
    DateOnly WorkDate,
    DateTimeOffset At,
    string? ActorEmail,
    DateTimeOffset? VoidedAt,
    string? VoidReason);

public sealed record ProductionWorkImportInput(IReadOnlyList<ProductionWorkImportRow> Rows);

public sealed record ProductionWorkImportResult(
    bool DryRun, int Created, int Unchanged, int Errors, IReadOnlyList<ProductionWorkImportLine> Lines);

/// <param name="Action">created, unchanged or error.</param>
public sealed record ProductionWorkImportLine(
    string LegacyId, int? OrderNumber, int Position, string EmployeeName, string Kind, int Quantity, string Action,
    string? Error);
