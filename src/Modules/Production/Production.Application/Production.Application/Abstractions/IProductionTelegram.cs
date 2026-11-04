namespace Production.Application.Abstractions;

/// <summary>The Brevi bot, as the workshop needs it; implemented in the host over the Reminders bot.</summary>
public interface IProductionTelegram
{
    /// <summary>The server has the bot token.</summary>
    bool IsConfigured { get; }

    /// <param name="threadId">A topic of a group with topics; null posts to the main chat.</param>
    /// <returns>The Telegram message id.</returns>
    Task<long> SendAsync(long chatId, int? threadId, string html, CancellationToken cancellationToken);

    /// <summary>Groups the bot is in, to choose the workshop group.</summary>
    Task<IReadOnlyList<ProductionTelegramChat>> ListChatsAsync(CancellationToken cancellationToken);
}

public sealed record ProductionTelegramChat(long ChatId, string Title);

/// <summary>Thrown by <see cref="IProductionTelegram.SendAsync"/> when Telegram refuses or cannot be reached.</summary>
public sealed class ProductionTelegramException(string message, Exception? inner = null) : Exception(message, inner);

/// <summary>Where the admin is, for the link in the Telegram message.</summary>
public sealed record ProductionLinks(string AdminUrl)
{
    public string Order(int orderId) => $"{AdminUrl.TrimEnd('/')}/production/orders/{orderId}";
}
