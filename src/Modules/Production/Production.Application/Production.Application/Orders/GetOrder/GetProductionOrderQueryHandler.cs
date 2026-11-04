using Production.Application.Abstractions;
using Production.Application.Contracts;

namespace Production.Application.Orders.GetOrder;

public sealed class GetProductionOrderQueryHandler(IProductionDbContext db)
    : IQueryHandler<GetProductionOrderQuery, Result<ProductionOrderResponse>>
{
    public async ValueTask<Result<ProductionOrderResponse>> Handle(GetProductionOrderQuery query,
        CancellationToken cancellationToken)
    {
        var order = await db.Orders.AsNoTracking().Include(x => x.Items)
            .SingleOrDefaultAsync(x => x.Id == query.Id && x.DeletedAt == null, cancellationToken);
        return order is null
            ? Result<ProductionOrderResponse>.NotFound(OrderMessages.NotFound)
            : Result.Success(order.ToResponse(query.Actor));
    }
}
