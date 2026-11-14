using Production.Domain.Orders;
using Production.Application.Features.Orders.Shared.Validation;

namespace Production.Application.Features.Orders.Commands.CancelProductionShipment;

public sealed class CancelProductionShipmentValidator : AbstractValidator<CancelProductionShipmentCommand>
{
    public CancelProductionShipmentValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.OrderId).GreaterThan(0);
        RuleFor(x => x.ShipmentId).GreaterThan(0);
        RuleFor(x => x.Version).GreaterThan(0);
    }
}
