using Production.Domain.Orders;
using Production.Application.Features.Orders.Shared.Validation;

namespace Production.Application.Features.Orders.Commands.ReopenProductionOrder;

public sealed class ReopenProductionOrderValidator : AbstractValidator<ReopenProductionOrderCommand>
{
    public ReopenProductionOrderValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.OrderId).GreaterThan(0);
        RuleFor(x => x.Version).GreaterThan(0);
    }
}
