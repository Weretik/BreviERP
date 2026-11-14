using System.Net;
using System.Text;
using BuildingBlocks.Application.Outbox;
using Crm.Application.SiteRequests.Abstractions;
using Crm.Domain.SiteRequests;

namespace Crm.Application.SiteRequests.Notify;

public sealed record NotifySiteRequestPayload(long RequestId);

/// <summary>
/// Posts a saved site request to the bot's groups (LEAD-002 R-04, PLT-003 R-07). A request already posted is skipped,
/// so a repeated message sends nothing. When no group got it the reason stays on the request and the outbox tries
/// again, so the request is posted once Telegram or the «Нагадування» setting is back.
/// </summary>
public sealed class NotifySiteRequestOutboxHandler(
    ISiteRequestStore store, ISiteRequestNotifier notifier, SiteRequestLinks links, TimeProvider clock,
    ILogger<NotifySiteRequestOutboxHandler> logger) : IOutboxHandler
{
    public const string MessageType = "crm.site-request.notify";

    public string Type => MessageType;

    public async Task<string?> HandleAsync(string payload, CancellationToken cancellationToken)
    {
        var id = OutboxMessage.Read<NotifySiteRequestPayload>(payload).RequestId;
        var request = await store.SiteRequests.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (request is null || request.NotifiedAt is not null) return null;

        string? error;
        try
        {
            error = await notifier.NotifyAsync(Message(request, links), cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning(exception, "Site request {Id} was not posted to Telegram", request.Id);
            error = "Telegram недоступний.";
        }

        request.Notified(error is null ? clock.GetUtcNow() : null, error);
        await store.SaveChangesAsync(CancellationToken.None);
        return error;
    }

    /// <summary>The Telegram card of a new request, in Telegram HTML.</summary>
    public static string Message(SiteRequest request, SiteRequestLinks links)
    {
        static string E(string value) => WebUtility.HtmlEncode(value);
        var text = new StringBuilder();
        text.Append("🆕 <b>Заявка з сайту</b> №").Append(request.Id).Append('\n');
        text.Append("📞 <b>").Append(E(request.Phone)).Append("</b>\n");
        if (request.Workwear is not null) text.Append("Що пошити: ").Append(E(request.Workwear)).Append('\n');
        if (request.Quantity is not null) text.Append("Кількість: ").Append(E(request.Quantity)).Append('\n');
        if (request.Comment is not null) text.Append("Коментар: ").Append(E(request.Comment)).Append('\n');
        if (request.FileUrl is not null)
            text.Append("Файл: <a href=\"").Append(E(request.FileUrl)).Append("\">")
                .Append(E(request.FileName ?? "файл")).Append("</a>\n");
        if (request.Locale == "ru") text.Append("Сторінка російською\n");
        text.Append("<a href=\"").Append(E(links.List)).Append("\">Відкрити в адмінці</a>");
        return text.ToString();
    }
}
