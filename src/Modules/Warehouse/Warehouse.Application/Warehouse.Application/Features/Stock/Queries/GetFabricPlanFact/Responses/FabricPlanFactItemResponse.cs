namespace Warehouse.Application.Features.Stock.Queries.GetFabricPlanFact.Responses;

/// <summary>An item in the details: its main-fabric plan and fact, and every material by role.</summary>
/// <param name="Reason">Why the item cannot be compared; null when it can.</param>
/// <param name="IssuedOn">The day of its last issue: the period of the report goes by it.</param>
public sealed record FabricPlanFactItemResponse(
    int OrderId,
    int OrderNumber,
    string ClientName,
    int ItemId,
    int Products,
    decimal? Plan,
    decimal Fact,
    decimal? Difference,
    string Result,
    string? Reason,
    IReadOnlyList<FabricPlanFactMaterialResponse> Materials,
    DateOnly? IssuedOn = null);
