using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;
using Warehouse.Application.Features.Stock.Queries.GetFabricStock.Results;

namespace Warehouse.Application.Features.Stock.Queries.GetFabricStock;

/// <summary>
/// «Залишки»: fabrics by canonical code with every colour the stock holds, lines of an alias group and colour summed.
/// Only fabrics and colours with stock lines appear; zero and negative balances stay. Each pair also carries what open
/// plans book of it («У броні», «Вільно», what to order), the tasks behind that and its latest movement (WH-008).
/// Search and the problem filters are on the page.
/// </summary>
public sealed record GetFabricStockQuery(WarehouseActor Actor) : IQuery<Result<FabricStockListResponse>>;
