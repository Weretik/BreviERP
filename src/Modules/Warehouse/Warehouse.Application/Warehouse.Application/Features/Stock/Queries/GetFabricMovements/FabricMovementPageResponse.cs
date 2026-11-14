using Warehouse.Application.Features.Stock.Shared;
namespace Warehouse.Application.Features.Stock.Queries.GetFabricMovements;

/// <summary>A page of 50 movements, newest operation date first; <c>Total</c> is the rows of the chosen kind.</summary>
public sealed record FabricMovementPageResponse(
    IReadOnlyList<FabricMovementRowResponse> Rows,
    int Page,
    bool HasMore,
    FabricStockPermissions Permissions,
    int Total = 0,
    FabricMovementCountsResponse? Counts = null);
