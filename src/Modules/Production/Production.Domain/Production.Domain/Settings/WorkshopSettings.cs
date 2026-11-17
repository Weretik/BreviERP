using BuildingBlocks.Domain.Entity;
using BuildingBlocks.Domain.Events;
using BuildingBlocks.Domain.Results;
using Production.Domain.Orders;
using Production.Domain.Orders.Rules;

namespace Production.Domain.Settings;

/// <summary>
/// «Налаштування цеху»: one row. Where the bot posts new orders: the workshop group and, in a group with topics, the
/// topic. Sending stays off until someone turns it on.
/// </summary>
public sealed class WorkshopSettings : DomainEventSource, IAggregateRoot
{
    public const int SingletonId = 1;

    private WorkshopSettings() { }

    private WorkshopSettings(long? chatId, int? threadId)
    {
        Id = SingletonId;
        TelegramChatId = chatId;
        TelegramThreadId = threadId;
    }

    public static WorkshopSettings Create(long? chatId, int? threadId) =>
        new(chatId, threadId);

    public int Id { get; private set; }
    public bool TelegramEnabled { get; private set; }
    public long? TelegramChatId { get; private set; }
    public int? TelegramThreadId { get; private set; }
    public string? UpdatedByName { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public bool CanSendTelegram => TelegramEnabled && TelegramChatId is not null;

    public DomainResult Update(bool enabled, long? chatId, int? threadId, string actorName, DateTimeOffset now)
    {
        if (enabled && chatId is null)
            return Error.Validation("production.telegram_chat_required", "Оберіть групу Telegram, щоб увімкнути надсилання.",
                "telegramChatId");
        TelegramEnabled = enabled;
        TelegramChatId = chatId;
        TelegramThreadId = threadId;
        UpdatedByName = OrderText.Required(actorName, ProductionOrder.PersonMaxLength);
        UpdatedAt = now;
        Raise(new WorkshopSettingsChanged(enabled));
        return DomainResult.Success();
    }
}
