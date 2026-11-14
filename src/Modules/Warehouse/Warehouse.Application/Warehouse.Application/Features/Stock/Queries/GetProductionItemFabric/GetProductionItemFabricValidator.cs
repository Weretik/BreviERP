using Warehouse.Application.Abstractions;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Queries.GetProductionItemFabric;

public sealed class GetProductionItemFabricValidator : AbstractValidator<GetProductionItemFabricQuery>
{
    public GetProductionItemFabricValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.OrderId).GreaterThan(0);
        RuleFor(x => x.ItemId).GreaterThan(0);
    }
}
