using BuildingBlocks.Domain.Errors;
using BuildingBlocks.Domain.Results;

namespace Production.Domain.Orders;

/// <summary>«Не надіслано», «Надсилається», «Надіслано», «Помилка».</summary>
public enum TelegramDeliveryStatus
{
    NotSent = 1,
    Sending = 2,
    Sent = 3,
    Failed = 4
}

/// <summary>
/// The one message to the workshop group about an order. <see cref="Begin"/> moves it to «Надсилається» first and the
/// row is a concurrency token, so two presses cannot send it twice; a sent message is never sent again.
/// </summary>
public sealed class ProductionTelegramDelivery
{
    public const int ErrorMaxLength = 500;

    private ProductionTelegramDelivery() { }

    public ProductionTelegramDelivery(int orderId)
    {
        OrderId = orderId;
        Status = TelegramDeliveryStatus.NotSent;
    }

    public int OrderId { get; private set; }
    public TelegramDeliveryStatus Status { get; private set; }
    public long? ChatId { get; private set; }
    public int? ThreadId { get; private set; }
    public long? MessageId { get; private set; }
    public int Attempts { get; private set; }
    public string? LastError { get; private set; }
    public string? SentByName { get; private set; }
    public DateTimeOffset? SentAt { get; private set; }
    public int Version { get; private set; }

    public bool CanSend => Status is TelegramDeliveryStatus.NotSent or TelegramDeliveryStatus.Failed;

    public DomainResult Begin(long chatId, int? threadId, string actorName)
    {
        if (!CanSend)
            return Error.Conflict("production.telegram_already_sent", "Повідомлення вже надіслано або його не можна надіслати.");
        Status = TelegramDeliveryStatus.Sending;
        ChatId = chatId;
        ThreadId = threadId;
        SentByName = OrderText.Required(actorName, ProductionOrder.PersonMaxLength);
        Attempts++;
        Version++;
        return DomainResult.Success();
    }

    public void Succeed(long messageId, DateTimeOffset now)
    {
        Status = TelegramDeliveryStatus.Sent;
        MessageId = messageId;
        SentAt = now;
        LastError = null;
        Version++;
    }

    public void Fail(string error)
    {
        Status = TelegramDeliveryStatus.Failed;
        LastError = error.Length > ErrorMaxLength ? error[..ErrorMaxLength] : error;
        Version++;
    }

    /// <summary>An order moved from the old «Цех» whose message went out there.</summary>
    public void MarkSentByOldApp(DateTimeOffset? sentAt)
    {
        Status = TelegramDeliveryStatus.Sent;
        SentAt = sentAt;
        SentByName = "Старий «Цех»";
        Version++;
    }
}
