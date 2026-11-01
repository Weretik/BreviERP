namespace Reminders.Application.Abstractions;

/// <summary>Bot API operations the module needs. Texts use Telegram HTML formatting.</summary>
public interface ITelegramGateway
{
    bool IsConfigured { get; }
    Task<long> SendMessageAsync(long chatId, string html, TelegramKeyboard? keyboard, CancellationToken cancellationToken);
    Task EditMessageAsync(long chatId, long messageId, string html, TelegramKeyboard? keyboard, CancellationToken cancellationToken);
    Task EditKeyboardAsync(long chatId, long messageId, TelegramKeyboard? keyboard, CancellationToken cancellationToken);
    Task AnswerCallbackAsync(string callbackQueryId, string? text, CancellationToken cancellationToken);
    Task SetWebhookAsync(string url, string secretToken, CancellationToken cancellationToken);
}
