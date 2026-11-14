namespace Warehouse.Application.Features.Stock.Queries.GetFabricMovements.Results;

/// <summary>Movements of each kind chip of «Рух тканини» under the search, period, fabric and colour.</summary>
public sealed record FabricMovementCountsResponse(
    int All,
    int Receipt,
    int ItemIssue,
    int OneOffIssue,
    int Adjustment,
    int Opening,
    int Cancelled);
