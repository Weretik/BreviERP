using Production.Domain.Orders;
using Production.Application.Features.Orders.Shared;

namespace Production.Application.Features.Orders.Commands.CloseProductionOrder;

public sealed class CloseProductionOrderHandler(OrderTracking tracking)
    : ICommandHandler<CloseProductionOrderCommand, Result<ProductionOrderResponse>>
{
    public async ValueTask<Result<ProductionOrderResponse>> Handle(CloseProductionOrderCommand command,
        CancellationToken cancellationToken) =>
        await tracking.ChangeAsync(command.Actor, command.OrderId, command.Version,
            order => command.Actor.CanTrack(order) ? null : OrderMessages.CannotTrack,
            (order, actorName, now) => order.Close(actorName, now), cancellationToken);
}
