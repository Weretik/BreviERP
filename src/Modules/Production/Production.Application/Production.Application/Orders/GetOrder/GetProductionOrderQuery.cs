using Production.Application.Contracts;
using Production.Application.Orders.Access;

namespace Production.Application.Orders.GetOrder;

public sealed record GetProductionOrderQuery(ProductionActor Actor, int Id) : IQuery<Result<ProductionOrderResponse>>;

public sealed class GetProductionOrderQueryValidator : AbstractValidator<GetProductionOrderQuery>
{
    public GetProductionOrderQueryValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
