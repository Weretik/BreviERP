using Production.Domain.Orders;
using Production.Application.Features.Orders.Shared;

namespace Production.Application.Features.Orders.Commands.CancelProductionOrder;

public sealed class CancelProductionOrderHandler(OrderTracking tracking)
    : ICommandHandler<CancelProductionOrderCommand, Result<ProductionOrderResponse>>
{
    public async ValueTask<Result<ProductionOrderResponse>> Handle(CancelProductionOrderCommand command,
        CancellationToken cancellationToken) =>
        await tracking.ChangeAsync(command.Actor, command.OrderId, command.Input.Version,
            order => command.Actor.CanEdit(order) ? null : OrderMessages.CannotCancel,
            (order, actorName, now) => order.Cancel(command.Input.Reason, actorName, now), cancellationToken);
}
