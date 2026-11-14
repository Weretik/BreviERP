namespace Warehouse.Application.Features.Stock.Queries.GetFabricStockChoices;

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
