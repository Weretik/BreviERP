using Production.Application.Contracts;
using Production.Application.Orders.Access;
using Production.Application.Orders.Validation;
using Production.Domain.Orders;

namespace Production.Application.Orders.Tracking;

/// <summary>«Відвантажити»: part of an item goes to the client; more than is left is refused.</summary>
public sealed record CreateProductionShipmentCommand(
    ProductionActor Actor, int OrderId, int ItemId, CreateProductionShipmentInput Input)
    : ICommand<Result<ProductionOrderResponse>>;

public sealed class CreateProductionShipmentCommandValidator : AbstractValidator<CreateProductionShipmentCommand>
{
    public CreateProductionShipmentCommandValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.OrderId).GreaterThan(0);
        RuleFor(x => x.ItemId).GreaterThan(0);
        RuleFor(x => x.Input).NotNull();
        RuleFor(x => x.Input.Version).GreaterThan(0).When(x => x.Input is not null);
        RuleFor(x => x.Input.Quantity).InclusiveBetween(1, ProductionItem.MaxQuantity)
            .WithMessage("Кількість — ціле число від 1.").When(x => x.Input is not null);
        RuleFor(x => x.Input.ShippedOn).NotEmpty().WithMessage("Вкажіть дату відвантаження.")
            .When(x => x.Input is not null);
        RuleFor(x => x.Input.Comment).MaximumLength(ProductionShipment.CommentMaxLength)
            .When(x => x.Input is not null);
    }
}

public sealed class CreateProductionShipmentCommandHandler(OrderTracking tracking)
    : ICommandHandler<CreateProductionShipmentCommand, Result<ProductionOrderResponse>>
{
    public async ValueTask<Result<ProductionOrderResponse>> Handle(CreateProductionShipmentCommand command,
        CancellationToken cancellationToken)
    {
        var input = command.Input;
        return await tracking.ChangeAsync(command.Actor, command.OrderId, input.Version,
            order => command.Actor.CanTrack(order) ? null : OrderMessages.CannotTrack,
            (order, actorName, now) =>
            {
                if (order.Items.All(x => x.Id != command.ItemId)) return Result.NotFound(OrderMessages.ItemNotInOrder);
                if (input.Quantity > order.RemainingToShip(command.ItemId))
                    return Result.Conflict(OrderMessages.TooMuchShipped);
                order.Ship(command.ItemId, input.Quantity, input.ShippedOn, input.Comment, actorName, now);
                return null;
            }, cancellationToken);
    }
}

/// <summary>Removes a wrongly recorded shipment; the history keeps it.</summary>
public sealed record CancelProductionShipmentCommand(ProductionActor Actor, int OrderId, int ShipmentId, int Version)
    : ICommand<Result<ProductionOrderResponse>>;

public sealed class CancelProductionShipmentCommandValidator : AbstractValidator<CancelProductionShipmentCommand>
{
    public CancelProductionShipmentCommandValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.OrderId).GreaterThan(0);
        RuleFor(x => x.ShipmentId).GreaterThan(0);
        RuleFor(x => x.Version).GreaterThan(0);
    }
}

public sealed class CancelProductionShipmentCommandHandler(OrderTracking tracking)
    : ICommandHandler<CancelProductionShipmentCommand, Result<ProductionOrderResponse>>
{
    public async ValueTask<Result<ProductionOrderResponse>> Handle(CancelProductionShipmentCommand command,
        CancellationToken cancellationToken) =>
        await tracking.ChangeAsync(command.Actor, command.OrderId, command.Version,
            order => command.Actor.CanTrack(order) ? null : OrderMessages.CannotTrack,
            (order, actorName, now) => order.CancelShipment(command.ShipmentId, actorName, now)
                ? null
                : Result.NotFound(OrderMessages.ShipmentNotFound),
            cancellationToken);
}
