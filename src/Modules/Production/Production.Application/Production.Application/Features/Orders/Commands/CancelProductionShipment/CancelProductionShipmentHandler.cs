using Production.Domain.Orders;
using Production.Application.Features.Orders.Shared;

namespace Production.Application.Features.Orders.Commands.CancelProductionShipment;

public sealed class CancelProductionShipmentHandler(OrderTracking tracking)
    : ICommandHandler<CancelProductionShipmentCommand, Result<ProductionOrderResponse>>
{
    public async ValueTask<Result<ProductionOrderResponse>> Handle(CancelProductionShipmentCommand command,
        CancellationToken cancellationToken) =>
        await tracking.ChangeAsync(command.Actor, command.OrderId, command.Version,
            order => command.Actor.CanTrack(order) ? null : OrderMessages.CannotTrack,
            (order, actorName, now) => order.CancelShipment(command.ShipmentId, actorName, now),
            cancellationToken);
}
