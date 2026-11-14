using Warehouse.Application.Abstractions;
using Warehouse.Application.Features.Stock.Shared;
using Warehouse.Application.Features.Stock.Shared.Plans;

namespace Warehouse.Application.Features.Stock.Queries.GetProductionItemFabric;

public sealed class GetProductionItemFabricHandler(IWarehouseProductionReader production, ItemFabricCards cards)
    : IQueryHandler<GetProductionItemFabricQuery, Result<ProductionItemFabricResponse>>
{
    public async ValueTask<Result<ProductionItemFabricResponse>> Handle(GetProductionItemFabricQuery query,
        CancellationToken cancellationToken)
    {
        var item = await production.ItemAsync(query.OrderId, query.ItemId, cancellationToken);
        if (item is null) return Result<ProductionItemFabricResponse>.NotFound(StockMessages.ItemNotFound);
        return Result.Success(await cards.BuildAsync(item, query.Actor, cancellationToken));
    }
}
