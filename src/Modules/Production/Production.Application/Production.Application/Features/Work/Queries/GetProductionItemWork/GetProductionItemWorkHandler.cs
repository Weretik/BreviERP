using Production.Application.Abstractions;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Validation;
using Production.Application.Features.Work.Shared;

namespace Production.Application.Features.Work.Queries.GetProductionItemWork;

public sealed class GetProductionItemWorkHandler(IProductionDbContext db, WorkCards cards)
    : IQueryHandler<GetProductionItemWorkQuery, Result<ProductionItemWorkResponse>>
{
    public async ValueTask<Result<ProductionItemWorkResponse>> Handle(GetProductionItemWorkQuery query,
        CancellationToken cancellationToken)
    {
        if (!query.Actor.SeesWork) return Result<ProductionItemWorkResponse>.Forbidden(WorkMessages.CannotSee);
        var order = await db.Orders.AsNoTracking().Include(x => x.Items)
            .SingleOrDefaultAsync(x => x.Id == query.OrderId && x.DeletedAt == null, cancellationToken);
        var item = order?.Items.FirstOrDefault(x => x.Id == query.ItemId);
        if (order is null || item is null) return Result<ProductionItemWorkResponse>.NotFound(OrderMessages.NotFound);
        return Result.Success(await cards.BuildAsync(order, item, query.Actor, cancellationToken));
    }
}
