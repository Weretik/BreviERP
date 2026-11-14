namespace Warehouse.Application.Features.Stock.Commands.TransferStock;

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
