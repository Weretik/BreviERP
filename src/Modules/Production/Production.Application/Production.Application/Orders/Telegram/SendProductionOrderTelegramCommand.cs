using Production.Application.Abstractions;
using Production.Application.Contracts;
using Production.Application.Orders.Access;
using Production.Application.Orders.Validation;
using Production.Domain.Orders;
using Production.Domain.Settings;

namespace Production.Application.Orders.Telegram;

/// <summary>
/// «Надіслати в цех»: one message to the workshop group with the items, files and a link to the order. A sent
/// message is never sent again; a failed one can be retried.
/// </summary>
public sealed record SendProductionOrderTelegramCommand(ProductionActor Actor, int OrderId)
    : ICommand<Result<ProductionOrderResponse>>;

public sealed class SendProductionOrderTelegramCommandValidator : AbstractValidator<SendProductionOrderTelegramCommand>
{
    public SendProductionOrderTelegramCommandValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.OrderId).GreaterThan(0);
    }
}

public sealed class SendProductionOrderTelegramCommandHandler(
    IProductionDbContext db, IProductionStaffReader staff, IProductionTelegram telegram, ProductionLinks links,
    OrderCards cards, TimeProvider clock, ILogger<SendProductionOrderTelegramCommandHandler> logger)
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

        var chatId = settings.TelegramChatId!.Value;
        delivery.Begin(chatId, settings.TelegramThreadId, actorName);
        try
        {
            // The «Надсилається» mark is saved first: a second press then finds it and does not send again.
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Result<ProductionOrderResponse>.Conflict(OrderMessages.SendingInProgress);
        }

        var files = await db.OrderFiles.AsNoTracking()
            .Where(x => x.OrderId == order.Id && x.DeletedAt == null)
            .OrderBy(x => x.Id).ToListAsync(cancellationToken);
        var text = ProductionTelegramMessage.Build(order, files, links.Order(order.Id));
        var now = clock.GetUtcNow();
        try
        {
            var messageId = await telegram.SendAsync(chatId, settings.TelegramThreadId, text, CancellationToken.None);
            delivery.Succeed(messageId, now);
            order.RecordAbout(null, "Telegram", null, "Надіслано в цех", actorName, now);
        }
        catch (ProductionTelegramException error)
        {
            logger.LogWarning("Telegram message for production order {Number} failed: {Error}", order.Number,
                error.Message);
            delivery.Fail(error.Message);
        }

        await db.SaveChangesAsync(CancellationToken.None);
        return Result.Success(await cards.BuildAsync(order, actor, cancellationToken));
    }
}
