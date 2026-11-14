using Warehouse.Application.Features.Stock.Shared;
namespace Warehouse.Application.Features.Stock.Queries.GetFabricStock.Results;

/// <summary>«Залишки»: only fabrics and colours that have stock lines, zero and negative included.</summary>
/// <param name="Short">Pairs whose stock does not cover the open plans (<c>ToOrder</c> above zero).</param>
public sealed record FabricStockListResponse(
    IReadOnlyList<FabricStockFabricResponse> Fabrics,
    decimal Meters,
    int Pairs,
    int Negative,
    FabricStockPermissions Permissions,
    int Short = 0);
