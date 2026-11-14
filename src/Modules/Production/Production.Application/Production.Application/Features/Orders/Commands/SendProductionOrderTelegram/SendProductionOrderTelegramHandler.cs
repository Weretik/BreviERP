using BuildingBlocks.Application.Outbox;
using Production.Application.Abstractions;
using Production.Domain.Orders;
using Production.Domain.Settings;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Validation;
using Production.Application.Features.Orders.Shared.Responses;

namespace Production.Application.Features.Orders.Commands.SendProductionOrderTelegram;

public sealed class SendProductionOrderTelegramHandler(
    IProductionDbContext db, IProductionStaffReader staff, IProductionTelegram telegram, OrderCards cards,
    IOutboxDispatcher outbox, TimeProvider clock)
    : ICommandHandler<SendProductionOrderTelegramCommand, Result<ProductionOrderResponse>>
{
    public async ValueTask<Result<ProductionOrderResponse>> Handle(SendProductionOrderTelegramCommand command,
        CancellationToken cancellationToken)
    {
        var actor = command.Actor;
        var order = await cards.Orders.SingleOrDefaultAsync(x => x.Id == command.OrderId, cancellationToken);
        if (order is null) return Result<ProductionOrderResponse>.NotFound(OrderMessages.NotFound);
        if (!actor.CanEdit(order)) return Result<ProductionOrderResponse>.Forbidden(OrderMessages.CannotEdit);
        var settings = await db.Settings.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == WorkshopSettings.SingletonId, cancellationToken);
        if (!telegram.IsConfigured || settings is not { CanSendTelegram: true })
            return Result<ProductionOrderResponse>.Conflict(OrderMessages.TelegramNotReady);
        var actorName = await OrderActors.NameAsync(staff, actor.UserId, cancellationToken);
        if (actorName is null) return Result<ProductionOrderResponse>.Forbidden(OrderMessages.ActorUnknown);

        var delivery = await db.TelegramDeliveries.SingleOrDefaultAsync(x => x.OrderId == order.Id, cancellationToken);
        if (delivery is null)
        {
            delivery = new ProductionTelegramDelivery(order.Id);
            db.TelegramDeliveries.Add(delivery);
        }

        if (!delivery.CanSend)
            return Result<ProductionOrderResponse>.Conflict(delivery.Status == TelegramDeliveryStatus.Sending
                ? OrderMessages.SendingInProgress
                : OrderMessages.CannotSend);

        var begun = delivery.Begin(settings.TelegramChatId!.Value, settings.TelegramThreadId, actorName);
        if (begun.IsFailure) return begun.Error!.ToResult<ProductionOrderResponse>();
        // «Надсилається» and the outbox message are saved together: a second press finds the mark and does not send
        // again, and a stop before the send leaves the message for the next dispatch.
        var message = OutboxMessage.Create(SendProductionOrderTelegramOutboxHandler.MessageType,
            new SendProductionOrderTelegramPayload(order.Id), clock.GetUtcNow());
        db.OutboxMessages.Add(message);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Result<ProductionOrderResponse>.Conflict(OrderMessages.SendingInProgress);
        }

        await outbox.DispatchAsync([message.Id], CancellationToken.None);
        // The send ran in its own scope; read the order as it is now.
        var current = await cards.Orders.AsNoTracking().SingleAsync(x => x.Id == order.Id, CancellationToken.None);
        return Result.Success(await cards.BuildAsync(current, actor, cancellationToken));
    }
}
