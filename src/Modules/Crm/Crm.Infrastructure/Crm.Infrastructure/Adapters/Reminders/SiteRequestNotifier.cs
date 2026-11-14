
using Reminders.Contracts.Telegram;
using Crm.Application.Abstractions.SiteRequests;

namespace Crm.Infrastructure.Adapters.Reminders;

/// <summary>
/// Posts a site request through the Brevi bot of «Нагадування» to every approved group ticked «Заявки з сайту»
/// (LEAD-002 R-05).
/// </summary>
internal sealed class SiteRequestNotifier(ITelegramBot bot, ILogger<SiteRequestNotifier> logger) : ISiteRequestNotifier
{
    public const string NotConfigured = "Бот Telegram не налаштований на сервері.";
    public const string NoChat = "На сторінці «Нагадування» не вибрано групу для заявок з сайту.";

    public async Task<string?> NotifyAsync(string html, CancellationToken cancellationToken)
    {
        if (!bot.IsConfigured) return NotConfigured;
        var chats = await bot.SiteRequestChatIdsAsync(cancellationToken);
        if (chats.Count == 0) return NoChat;
        string? error = null;
        var sent = 0;
        foreach (var chatId in chats)
        {
            try
            {
                await bot.SendAsync(chatId, null, html, cancellationToken);
                sent++;
            }
            catch (Exception exception) when (exception is TelegramDeliveryException or HttpRequestException
                                                  or TaskCanceledException && !cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning(exception, "Site request was not posted to chat {ChatId}", chatId);
                error = exception is TelegramDeliveryException ? exception.Message : "Telegram недоступний.";
            }
        }

        return sent > 0 ? null : error;
    }
}
