using Production.Domain.Orders;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Responses;
using Production.Application.Features.Orders.Shared.Rules;

namespace Production.Application.Features.Orders.Commands.CreateProductionShipment;

public sealed class CreateProductionShipmentHandler(OrderTracking tracking)
    : ICommandHandler<CreateProductionShipmentCommand, Result<ProductionOrderResponse>>
{
    public async ValueTask<Result<ProductionOrderResponse>> Handle(CreateProductionShipmentCommand command,
        CancellationToken cancellationToken)
    {
        var input = command.Input;
        return await tracking.ChangeAsync(command.Actor, command.OrderId, input.Version,
            order => command.Actor.CanTrack(order) ? null : OrderMessages.CannotTrack,
            (order, actorName, now) =>
                order.Ship(command.ItemId, input.Quantity, input.ShippedOn, input.Comment, actorName, now).WithoutValue(),
            cancellationToken);
    }
}
