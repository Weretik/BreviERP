using Crm.Application.SiteRequests.Abstractions;
using Microsoft.EntityFrameworkCore;
using Reminders.Application.Abstractions;

namespace Host.Api.Crm;

/// <summary>
/// Posts a site request through the Brevi bot of «Нагадування» to every approved group ticked «Заявки з сайту»
/// (LEAD-002 R-05).
/// </summary>
internal sealed class SiteRequestNotifier(ITelegramGateway gateway, IRemindersDbContext reminders,
    ILogger<SiteRequestNotifier> logger) : ISiteRequestNotifier
{
    public const string NotConfigured = "Бот Telegram не налаштований на сервері.";
    public const string NoChat = "На сторінці «Нагадування» не вибрано групу для заявок з сайту.";

    public async Task<string?> NotifyAsync(string html, CancellationToken cancellationToken)
    {
        if (!gateway.IsConfigured) return NotConfigured;
        var chats = (await reminders.Chats.AsNoTracking()
                .Where(x => x.ReceivesSiteRequests && x.IsApproved && x.IsBotMember)
                .Select(x => x.TelegramChatId)
                .ToListAsync(cancellationToken))
            .Distinct().ToArray();
        if (chats.Length == 0) return NoChat;

        string? error = null;
        var sent = 0;
        foreach (var chatId in chats)
        {
            try
            {
                await gateway.SendTopicMessageAsync(chatId, null, html, cancellationToken);
                sent++;
            }
            catch (Exception exception) when (exception is TelegramGatewayException or HttpRequestException
                                                  or TaskCanceledException && !cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning(exception, "Site request was not posted to chat {ChatId}", chatId);
                error = exception is TelegramGatewayException ? exception.Message : "Telegram недоступний.";
            }
        }

        return sent > 0 ? null : error;
    }
}
