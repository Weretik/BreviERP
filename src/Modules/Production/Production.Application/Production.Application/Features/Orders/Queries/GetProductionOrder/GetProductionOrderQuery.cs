using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Responses;

namespace Production.Application.Features.Orders.Queries.GetProductionOrder;

public sealed record GetProductionOrderQuery(ProductionActor Actor, int Id) : IQuery<Result<ProductionOrderResponse>>;

public sealed class GetProductionOrderValidator : AbstractValidator<GetProductionOrderQuery>
{
    public GetProductionOrderValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
