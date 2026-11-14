using BuildingBlocks.Application.Outbox;
using Production.Application.Abstractions;
using Production.Domain.Orders;

namespace Production.Application.Features.Orders.Commands.SendProductionOrderTelegram;

public sealed record SendProductionOrderTelegramPayload(int OrderId);

/// <summary>
/// Sends the «Надіслати в цех» message of an order whose delivery is «Надсилається» (PLT-003 R-08). Any other state
/// means the work is done (sent, or failed and shown to the person), so a repeated message sends nothing. A Telegram
/// refusal marks the delivery failed with the reason, as before: the person sees it and presses again.
/// </summary>
public sealed class SendProductionOrderTelegramOutboxHandler(
    IProductionDbContext db, IProductionTelegram telegram, ProductionLinks links, TimeProvider clock,
    ILogger<SendProductionOrderTelegramOutboxHandler> logger) : IOutboxHandler
{
    public const string MessageType = "production.order.telegram";

    public string Type => MessageType;

    public async Task<string?> HandleAsync(string payload, CancellationToken cancellationToken)
    {
        var orderId = OutboxMessage.Read<SendProductionOrderTelegramPayload>(payload).OrderId;
        var delivery = await db.TelegramDeliveries.SingleOrDefaultAsync(x => x.OrderId == orderId, cancellationToken);
        if (delivery is not { Status: TelegramDeliveryStatus.Sending, ChatId: { } chatId }) return null;
        var order = await db.Orders.Include(x => x.Items)
            .SingleOrDefaultAsync(x => x.Id == orderId && x.DeletedAt == null, cancellationToken);
        if (order is null)
        {
            delivery.Fail("Замовлення видалено.");
            await db.SaveChangesAsync(CancellationToken.None);
            return null;
        }

        var files = await db.OrderFiles.AsNoTracking()
            .Where(x => x.OrderId == order.Id && x.DeletedAt == null)
            .OrderBy(x => x.Id).ToListAsync(cancellationToken);
        var text = ProductionTelegramMessage.Build(order, files, links.Order(order.Id));
        try
        {
            var messageId = await telegram.SendAsync(chatId, delivery.ThreadId, text, CancellationToken.None);
            var now = clock.GetUtcNow();
            delivery.Succeed(messageId, now);
            order.RecordAbout(null, "Telegram", null, "Надіслано в цех", delivery.SentByName ?? "", now);
        }
        catch (ProductionTelegramException error)
        {
            logger.LogWarning("Telegram message for production order {Number} failed: {Error}", order.Number,
                error.Message);
            delivery.Fail(error.Message);
        }

        await db.SaveChangesAsync(CancellationToken.None);
        return null;
    }
}
