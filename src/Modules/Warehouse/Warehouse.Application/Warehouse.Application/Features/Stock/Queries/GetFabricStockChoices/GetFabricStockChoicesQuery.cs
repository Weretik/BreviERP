using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Queries.GetFabricStockChoices;

/// <summary>
/// Active offers of «Тканини» with a FAB- code (alias groups share one group code), supplier names and the colours of
/// «Кольори» for the stock forms. Prices only for people who see them.
/// </summary>
public sealed record GetFabricStockChoicesQuery(WarehouseActor Actor) : IQuery<Result<FabricStockChoicesResponse>>;
