using Production.Application.Contracts;
using Production.Application.Orders.Access;
using Production.Application.Orders.Validation;
using Production.Domain.Orders;

namespace Production.Application.Orders.Tracking;

/// <summary>Ticks the етапи of an item; several may be ticked at once, as in the old «Цех».</summary>
public sealed record SetProductionItemStagesCommand(
    ProductionActor Actor, int OrderId, int ItemId, SetProductionItemStagesInput Input)
    : ICommand<Result<ProductionOrderResponse>>;

public sealed class SetProductionItemStagesCommandValidator : AbstractValidator<SetProductionItemStagesCommand>
{
    public SetProductionItemStagesCommandValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.OrderId).GreaterThan(0);
        RuleFor(x => x.ItemId).GreaterThan(0);
        RuleFor(x => x.Input).NotNull();
        RuleFor(x => x.Input.Version).GreaterThan(0).When(x => x.Input is not null);
        RuleFor(x => x.Input.Stages).NotNull().Must(x => x.Count <= 9).When(x => x.Input is not null);
        RuleForEach(x => x.Input.Stages)
            .Must(x => Enum.TryParse<ProductionStage>(x, true, out var stage) && Enum.IsDefined(stage))
            .WithMessage("Невідомий етап.")
            .When(x => x.Input?.Stages is not null);
    }
}

public sealed class SetProductionItemStagesCommandHandler(OrderTracking tracking)
    : ICommandHandler<SetProductionItemStagesCommand, Result<ProductionOrderResponse>>
{
    public async ValueTask<Result<ProductionOrderResponse>> Handle(SetProductionItemStagesCommand command,
        CancellationToken cancellationToken)
    {
        var stages = command.Input.Stages.Select(x => Enum.Parse<ProductionStage>(x, ignoreCase: true)).ToArray();
        return await tracking.ChangeAsync(command.Actor, command.OrderId, command.Input.Version,
            order => command.Actor.CanTrack(order) ? null : OrderMessages.CannotTrack,
            (order, actorName, now) =>
            {
                if (order.Items.All(x => x.Id != command.ItemId)) return Result.NotFound(OrderMessages.ItemNotInOrder);
                order.SetItemStages(command.ItemId, stages, actorName, now);
                return null;
            }, cancellationToken);
    }
}
