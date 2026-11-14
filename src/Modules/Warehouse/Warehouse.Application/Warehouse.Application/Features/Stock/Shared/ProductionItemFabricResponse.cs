namespace Warehouse.Application.Features.Stock.Shared;

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
