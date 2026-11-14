using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Queries.GetFabricValuation;

/// <summary>
/// «Вартість складу» (TZ §13): the positive balance of every fabric and colour at today's price of «Тканини». Each
/// request reads the balances and the prices anew («Оновити ціни і залишки»).
/// </summary>
public sealed record GetFabricValuationQuery(WarehouseActor Actor) : IQuery<Result<FabricValuationResponse>>;
