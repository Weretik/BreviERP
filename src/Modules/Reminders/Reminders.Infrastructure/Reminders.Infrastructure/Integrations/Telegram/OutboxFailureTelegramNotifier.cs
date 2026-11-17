using System.Net;
using BuildingBlocks.Application.Outbox;
using Microsoft.Extensions.Logging;
using Reminders.Contracts.Telegram;

namespace Reminders.Infrastructure.Integrations.Telegram;

/// <summary>
/// Posts an outbox message that stopped being retried to the groups ticked «Заявки з сайту» (EVT-07), so a lost site
/// request, production order or stock record is noticed the same day instead of only in the logs.
/// </summary>
internal sealed class OutboxFailureTelegramNotifier(ITelegramBot bot, ILogger<OutboxFailureTelegramNotifier> logger)
    : IOutboxFailureNotifier
{
    public const int ErrorPreviewLength = 500;

    public async Task NotifyGaveUpAsync(OutboxFailure failure, CancellationToken cancellationToken)
    {
        if (!bot.IsConfigured) return;
        var chats = await bot.SiteRequestChatIdsAsync(cancellationToken);
        if (chats.Count == 0)
        {
            logger.LogWarning("No group is ticked «Заявки з сайту»; outbox message {Id} that gave up is only logged", failure.Id);
            return;
        }

        var html = Text(failure);
        foreach (var chatId in chats)
            await bot.SendAsync(chatId, null, html, cancellationToken);
    }

    public static string Text(OutboxFailure failure)
    {
        var error = failure.LastError ?? "—";
        if (error.Length > ErrorPreviewLength) error = error[..ErrorPreviewLength] + "…";
        return $"""
                ⚠️ <b>Не вдалося виконати фонову дію</b>
                Дія: <code>{E(failure.Type)}</code>
                Спроб: {failure.Attempts}, більше не повторюється.
                Створено: {failure.CreatedAt:yyyy-MM-dd HH:mm} UTC
                Помилка: {E(error)}
                Ідентифікатор: <code>{failure.Id}</code>
                """;
    }

    private static string E(string value) => WebUtility.HtmlEncode(value);
}
