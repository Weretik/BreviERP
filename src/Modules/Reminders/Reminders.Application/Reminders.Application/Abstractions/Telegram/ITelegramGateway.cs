namespace Reminders.Application.Abstractions.Telegram;

/// <summary>Bot API operations the module needs. Texts use Telegram HTML formatting.</summary>
public interface ITelegramGateway
{
    bool IsConfigured { get; }
    Task<long> SendMessageAsync(long chatId, string html, TelegramKeyboard? keyboard, CancellationToken cancellationToken);

    /// <summary>A plain message to a topic of a group with topics (<paramref name="threadId"/>), or to the main chat when it is null.</summary>
    Task<long> SendTopicMessageAsync(long chatId, int? threadId, string html, CancellationToken cancellationToken);

    Task EditMessageAsync(long chatId, long messageId, string html, TelegramKeyboard? keyboard, CancellationToken cancellationToken);
    Task EditKeyboardAsync(long chatId, long messageId, TelegramKeyboard? keyboard, CancellationToken cancellationToken);
    Task AnswerCallbackAsync(string callbackQueryId, string? text, CancellationToken cancellationToken);
    Task SetWebhookAsync(string url, string secretToken, CancellationToken cancellationToken);
}
