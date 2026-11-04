using Production.Application.Abstractions;
using Production.Application.Contracts;
using Production.Application.Orders;
using Production.Application.Orders.Access;
using Production.Application.Orders.Validation;

namespace Production.Application.Stock.Issues;

/// <summary>Tab «Тканина» of an item; everyone who sees the order sees it.</summary>
public sealed record GetProductionItemFabricQuery(ProductionActor Actor, int OrderId, int ItemId)
    : IQuery<Result<ProductionItemFabricResponse>>;

public sealed class GetProductionItemFabricQueryValidator : AbstractValidator<GetProductionItemFabricQuery>
{
    public GetProductionItemFabricQueryValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.OrderId).GreaterThan(0);
        RuleFor(x => x.ItemId).GreaterThan(0);
    }
}

public sealed class GetProductionItemFabricQueryHandler(IProductionDbContext db, ItemFabricCards cards)
    : IQueryHandler<GetProductionItemFabricQuery, Result<ProductionItemFabricResponse>>
{
    public async ValueTask<Result<ProductionItemFabricResponse>> Handle(GetProductionItemFabricQuery query,
        CancellationToken cancellationToken)
    {
        var order = await db.Orders.AsNoTracking().Include(x => x.Items)
            .SingleOrDefaultAsync(x => x.Id == query.OrderId && x.DeletedAt == null, cancellationToken);
        var item = order?.Items.FirstOrDefault(x => x.Id == query.ItemId);
        if (order is null || item is null) return Result<ProductionItemFabricResponse>.NotFound(OrderMessages.NotFound);
        return Result.Success(await cards.BuildAsync(order, item, query.Actor, cancellationToken));
    }
}
