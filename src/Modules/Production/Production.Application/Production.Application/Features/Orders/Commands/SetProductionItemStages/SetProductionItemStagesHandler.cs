using Production.Domain.Orders;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Validation;

namespace Production.Application.Features.Orders.Commands.SetProductionItemStages;

public sealed class SetProductionItemStagesHandler(OrderTracking tracking)
    : ICommandHandler<SetProductionItemStagesCommand, Result<ProductionOrderResponse>>
{
    public async ValueTask<Result<ProductionOrderResponse>> Handle(SetProductionItemStagesCommand command,
        CancellationToken cancellationToken)
    {
        var stages = command.Input.Stages.Select(x => Enum.Parse<ProductionStage>(x, ignoreCase: true)).ToArray();
        return await tracking.ChangeAsync(command.Actor, command.OrderId, command.Input.Version,
            order => command.Actor.CanTrack(order) ? null : OrderMessages.CannotTrack,
            (order, actorName, now) => order.SetItemStages(command.ItemId, stages, actorName, now).WithoutValue(),
            cancellationToken);
    }
}
