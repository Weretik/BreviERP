namespace Reminders.Contracts.Telegram;

/// <summary>
/// The Brevi bot of «Нагадування» for other modules: posts to groups the bot is in. Texts use Telegram HTML formatting.
/// </summary>
public interface ITelegramBot
{
    /// <summary>False when the bot token is not configured on this server.</summary>
    bool IsConfigured { get; }

    /// <summary>
    /// Posts to a topic of a group with topics (<paramref name="threadId"/>), or to the main chat when it is null, and
    /// returns the message id. Telegram refusals throw <see cref="TelegramDeliveryException"/>; network failures throw
    /// <see cref="HttpRequestException"/> or <see cref="TaskCanceledException"/>.
    /// </summary>
    Task<long> SendAsync(long chatId, int? threadId, string html, CancellationToken cancellationToken);

    /// <summary>Groups the bot is a member of, by name.</summary>
    Task<IReadOnlyList<TelegramGroup>> ListGroupsAsync(CancellationToken cancellationToken);

    /// <summary>Approved groups with the bot that are ticked «Заявки з сайту».</summary>
    Task<IReadOnlyList<long>> SiteRequestChatIdsAsync(CancellationToken cancellationToken);
}

public sealed record TelegramGroup(long ChatId, string Title);

/// <summary>Telegram refused a message; <see cref="Exception.Message"/> is a short reason for the user.</summary>
public sealed class TelegramDeliveryException(string message, Exception? inner = null) : Exception(message, inner);
