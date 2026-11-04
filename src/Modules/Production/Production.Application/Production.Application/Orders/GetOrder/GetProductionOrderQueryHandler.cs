using Production.Application.Abstractions;
using Production.Application.Contracts;

namespace Production.Application.Orders.GetOrder;

public sealed class GetProductionOrderQueryHandler(OrderCards cards)
    : IQueryHandler<GetProductionOrderQuery, Result<ProductionOrderResponse>>
{
    public async ValueTask<Result<ProductionOrderResponse>> Handle(GetProductionOrderQuery query,
        CancellationToken cancellationToken)
    {
        var order = await cards.Orders.AsNoTracking().SingleOrDefaultAsync(x => x.Id == query.Id, cancellationToken);
        return order is null
            ? Result<ProductionOrderResponse>.NotFound(OrderMessages.NotFound)
            : Result.Success(await cards.BuildAsync(order, query.Actor, cancellationToken));
    }
}
