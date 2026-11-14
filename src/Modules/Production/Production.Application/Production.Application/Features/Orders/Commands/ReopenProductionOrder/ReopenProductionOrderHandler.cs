using Production.Domain.Orders;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Responses;
using Production.Application.Features.Orders.Shared.Rules;

namespace Production.Application.Features.Orders.Commands.ReopenProductionOrder;

public sealed class ReopenProductionOrderHandler(OrderTracking tracking)
    : ICommandHandler<ReopenProductionOrderCommand, Result<ProductionOrderResponse>>
{
    public async ValueTask<Result<ProductionOrderResponse>> Handle(ReopenProductionOrderCommand command,
        CancellationToken cancellationToken) =>
        await tracking.ChangeAsync(command.Actor, command.OrderId, command.Version,
            order => command.Actor.CanReopen(order) ? null : OrderMessages.CannotReopen,
            (order, actorName, now) => order.Reopen(actorName, now), cancellationToken);
}
