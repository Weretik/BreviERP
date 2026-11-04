using Production.Application.Contracts;
using Production.Application.Orders.Access;
using Production.Application.Orders.Validation;
using Production.Domain.Orders;

namespace Production.Application.Orders.Tracking;

/// <summary>«Закрити»: everything is shipped.</summary>
public sealed record CloseProductionOrderCommand(ProductionActor Actor, int OrderId, int Version)
    : ICommand<Result<ProductionOrderResponse>>;

/// <summary>«Скасувати» with a reason.</summary>
public sealed record CancelProductionOrderCommand(ProductionActor Actor, int OrderId, CancelProductionOrderInput Input)
    : ICommand<Result<ProductionOrderResponse>>;

/// <summary>«Повернути в роботу» (Admin).</summary>
public sealed record ReopenProductionOrderCommand(ProductionActor Actor, int OrderId, int Version)
    : ICommand<Result<ProductionOrderResponse>>;

public sealed class CloseProductionOrderCommandValidator : AbstractValidator<CloseProductionOrderCommand>
{
    public CloseProductionOrderCommandValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.OrderId).GreaterThan(0);
        RuleFor(x => x.Version).GreaterThan(0);
    }
}

public sealed class CancelProductionOrderCommandValidator : AbstractValidator<CancelProductionOrderCommand>
{
    public CancelProductionOrderCommandValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.OrderId).GreaterThan(0);
        RuleFor(x => x.Input).NotNull();
        RuleFor(x => x.Input.Version).GreaterThan(0).When(x => x.Input is not null);
        RuleFor(x => x.Input.Reason).NotEmpty().WithMessage("Вкажіть причину скасування.")
            .MaximumLength(ProductionOrder.CancelReasonMaxLength).When(x => x.Input is not null);
    }
}

public sealed class ReopenProductionOrderCommandValidator : AbstractValidator<ReopenProductionOrderCommand>
{
    public ReopenProductionOrderCommandValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.OrderId).GreaterThan(0);
        RuleFor(x => x.Version).GreaterThan(0);
    }
}

public sealed class ProductionOrderStatusCommandHandler(OrderTracking tracking) :
    ICommandHandler<CloseProductionOrderCommand, Result<ProductionOrderResponse>>,
    ICommandHandler<CancelProductionOrderCommand, Result<ProductionOrderResponse>>,
    ICommandHandler<ReopenProductionOrderCommand, Result<ProductionOrderResponse>>
{
    public async ValueTask<Result<ProductionOrderResponse>> Handle(CloseProductionOrderCommand command,
        CancellationToken cancellationToken) =>
        await tracking.ChangeAsync(command.Actor, command.OrderId, command.Version,
            order => command.Actor.CanTrack(order) ? null : OrderMessages.CannotTrack,
            (order, actorName, now) =>
            {
                if (!order.IsFullyShipped) return Result.Conflict(OrderMessages.NotFullyShipped);
                order.Close(actorName, now);
                return null;
            }, cancellationToken);

    public async ValueTask<Result<ProductionOrderResponse>> Handle(CancelProductionOrderCommand command,
        CancellationToken cancellationToken) =>
        await tracking.ChangeAsync(command.Actor, command.OrderId, command.Input.Version,
            order => command.Actor.CanEdit(order) ? null : OrderMessages.CannotCancel,
            (order, actorName, now) =>
            {
                order.Cancel(command.Input.Reason, actorName, now);
                return null;
            }, cancellationToken);

    public async ValueTask<Result<ProductionOrderResponse>> Handle(ReopenProductionOrderCommand command,
        CancellationToken cancellationToken) =>
        await tracking.ChangeAsync(command.Actor, command.OrderId, command.Version,
            order => command.Actor.CanReopen(order) ? null : OrderMessages.CannotReopen,
            (order, actorName, now) =>
            {
                order.Reopen(actorName, now);
                return null;
            }, cancellationToken);
}
