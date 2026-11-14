using Production.Domain.Orders;
using Production.Application.Features.Orders.Shared.Validation;

namespace Production.Application.Features.Orders.Commands.CloseProductionOrder;

public sealed class CloseProductionOrderValidator : AbstractValidator<CloseProductionOrderCommand>
{
    public CloseProductionOrderValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.OrderId).GreaterThan(0);
        RuleFor(x => x.Version).GreaterThan(0);
    }
}
