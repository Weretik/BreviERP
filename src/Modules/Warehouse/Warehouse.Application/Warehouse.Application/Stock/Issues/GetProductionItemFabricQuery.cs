using Warehouse.Application.Abstractions;
using Warehouse.Application.Access;
using Warehouse.Application.Contracts;

namespace Warehouse.Application.Stock.Issues;

/// <summary>Tab «Тканина» of an item; everyone who sees the order sees it.</summary>
public sealed record GetProductionItemFabricQuery(WarehouseActor Actor, int OrderId, int ItemId)
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

public sealed class GetProductionItemFabricQueryHandler(IWarehouseProductionReader production, ItemFabricCards cards)
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
