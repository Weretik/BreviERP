using Production.Application.Abstractions;
using Production.Application.Contracts;
using Production.Application.Orders;
using Production.Application.Orders.Access;
using Production.Application.Orders.Validation;

namespace Production.Application.Work.GetItemWork;

/// <summary>«Робота» of an item in the order card.</summary>
public sealed record GetProductionItemWorkQuery(ProductionActor Actor, int OrderId, int ItemId)
    : IQuery<Result<ProductionItemWorkResponse>>;

public sealed class GetProductionItemWorkQueryValidator : AbstractValidator<GetProductionItemWorkQuery>
{
    public GetProductionItemWorkQueryValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.OrderId).GreaterThan(0);
        RuleFor(x => x.ItemId).GreaterThan(0);
    }
}

public sealed class GetProductionItemWorkQueryHandler(IProductionDbContext db, WorkCards cards)
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
