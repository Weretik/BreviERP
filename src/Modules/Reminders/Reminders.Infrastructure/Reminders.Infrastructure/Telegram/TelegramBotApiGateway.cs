using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Reminders.Application.Abstractions;

namespace Reminders.Infrastructure.Telegram;

/// <summary>Minimal Telegram Bot API client over HttpClient. The token is part of the URL and is never logged.</summary>
public sealed class TelegramBotApiGateway(HttpClient http, IOptions<TelegramOptions> options) : ITelegramGateway
{
    private static readonly JsonSerializerOptions Json = new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
    private readonly TelegramOptions _options = options.Value;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_options.BotToken);

    public async Task<long> SendMessageAsync(long chatId, string html, TelegramKeyboard? keyboard, CancellationToken cancellationToken)
    {
        var result = await CallAsync("sendMessage", new
        {
            chat_id = chatId,
            text = html,
            parse_mode = "HTML",
            link_preview_options = new { is_disabled = true },
            reply_markup = Markup(keyboard)
        }, cancellationToken);
        return result.GetProperty("message_id").GetInt64();
    }

    public Task EditMessageAsync(long chatId, long messageId, string html, TelegramKeyboard? keyboard, CancellationToken cancellationToken) =>
        CallIgnoringUnchangedAsync("editMessageText", new
        {
            chat_id = chatId,
            message_id = messageId,
            text = html,
            parse_mode = "HTML",
            link_preview_options = new { is_disabled = true },
            reply_markup = Markup(keyboard)
        }, cancellationToken);

    public Task EditKeyboardAsync(long chatId, long messageId, TelegramKeyboard? keyboard, CancellationToken cancellationToken) =>
        CallIgnoringUnchangedAsync("editMessageReplyMarkup", new
        {
            chat_id = chatId,
            message_id = messageId,
            reply_markup = Markup(keyboard) ?? new { inline_keyboard = Array.Empty<object[]>() }
        }, cancellationToken);

    public Task AnswerCallbackAsync(string callbackQueryId, string? text, CancellationToken cancellationToken) =>
        CallAsync("answerCallbackQuery", new { callback_query_id = callbackQueryId, text }, cancellationToken);

    public Task SetWebhookAsync(string url, string secretToken, CancellationToken cancellationToken) =>
        CallAsync("setWebhook", new
        {
            url,
            secret_token = secretToken,
            allowed_updates = new[] { "message", "callback_query", "my_chat_member" }
        }, cancellationToken);

    private static object? Markup(TelegramKeyboard? keyboard) => keyboard is null ? null : new
    {
        inline_keyboard = keyboard.Rows.Select(row => row.Select(b => new { text = b.Text, callback_data = b.CallbackData }).ToArray()).ToArray()
    };

    private async Task CallIgnoringUnchangedAsync(string method, object body, CancellationToken cancellationToken)
    {
        try { await CallAsync(method, body, cancellationToken); }
        catch (TelegramApiException error) when (error.Description?.Contains("message is not modified", StringComparison.OrdinalIgnoreCase) == true)
        {
            // Same text and buttons: nothing to change.
        }
    }

    private async Task<JsonElement> CallAsync(string method, object body, CancellationToken cancellationToken)
    {
        if (!IsConfigured) throw new InvalidOperationException("Telegram bot token is not configured.");
        using var response = await http.PostAsJsonAsync(
            $"{_options.ApiBaseUrl.TrimEnd('/')}/bot{_options.BotToken}/{method}", body, Json, cancellationToken);
        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
        var root = document.RootElement;
        if (!root.TryGetProperty("ok", out var ok) || !ok.GetBoolean())
            throw new TelegramApiException(method, root.TryGetProperty("description", out var d) ? d.GetString() : response.StatusCode.ToString());
        return root.TryGetProperty("result", out var result) ? result.Clone() : default;
    }
}
