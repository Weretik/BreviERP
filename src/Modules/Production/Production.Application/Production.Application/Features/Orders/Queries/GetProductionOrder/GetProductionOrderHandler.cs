using BuildingBlocks.Domain.Identifiers.Orders;
using Production.Domain.Orders.ValueObjects;
using Production.Application.Abstractions;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Responses;

namespace Production.Application.Features.Orders.Queries.GetProductionOrder;

public sealed class GetProductionOrderHandler(OrderCards cards)
    : IQueryHandler<GetProductionOrderQuery, Result<ProductionOrderResponse>>
{
    public async ValueTask<Result<ProductionOrderResponse>> Handle(GetProductionOrderQuery query,
        CancellationToken cancellationToken)
    {
        var order = await cards.Orders.AsNoTracking().SingleOrDefaultAsync(x => x.Id == new ProductionOrderId(query.Id), cancellationToken);
        return order is null
            ? Result<ProductionOrderResponse>.NotFound(OrderMessages.NotFound)
            : Result.Success(await cards.BuildAsync(order, query.Actor, cancellationToken));
    }
}
