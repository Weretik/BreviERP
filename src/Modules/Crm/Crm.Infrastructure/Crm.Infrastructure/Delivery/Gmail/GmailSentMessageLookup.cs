using System.Net.Mail;
using System.Net.Http.Headers;
using System.Text.Json;
using Crm.Application.Delivery.Provider;
using Crm.Infrastructure.Delivery.Readiness;

namespace Crm.Infrastructure.Delivery.Gmail;

/// <summary>Read-only reconciliation. Never submits or permits an automatic resend.</summary>
internal sealed class GmailSentMessageLookup(HttpClient http, IQuoteDeliveryAccessTokenSource tokens,
    QuoteDeliveryProviderOptions options)
{
    private const string Root = "https://gmail.googleapis.com/gmail/v1/users/me/messages";
    public async Task<QuoteDeliveryProviderResult> ReconcileAsync(Guid deliveryId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!options.Enabled || options.ProviderKey != "gmail" || options.TimeoutSeconds is < 2 or > 60 || deliveryId == Guid.Empty || !MailAddress.TryCreate(options.SenderAddress, out _))
            return Unknown("gmail_reconciliation_unavailable");
        var token = await tokens.GetAccessTokenAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(token)) return Unknown("gmail_oauth_unavailable");
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(options.TimeoutSeconds));
        var identity = "<" + GmailMessageIdentity.For(deliveryId, options.SenderAddress) + ">";
        try
        {
            using var list = Request(Root + "?maxResults=2&labelIds=SENT&q=" + Uri.EscapeDataString("rfc822msgid:" + identity), token);
            using var response = await http.SendAsync(list, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
            if (!response.IsSuccessStatusCode) return Unknown("gmail_lookup_unavailable");
            using var json = await GmailResponseJson.ReadAsync(response, timeout.Token);
            if (json.RootElement.ValueKind != JsonValueKind.Object || json.RootElement.TryGetProperty("nextPageToken", out _) ||
                !json.RootElement.TryGetProperty("messages", out var messages) || messages.ValueKind != JsonValueKind.Array || messages.GetArrayLength() != 1)
                return Unknown("gmail_lookup_unconfirmed");
            var id = GmailResponseJson.MessageId(messages[0]);
            if (id is null) return Unknown("gmail_lookup_unconfirmed");
            using var get = Request(Root + "/" + Uri.EscapeDataString(id) + "?format=metadata&metadataHeaders=Message-ID", token);
            using var detail = await http.SendAsync(get, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
            if (!detail.IsSuccessStatusCode) return Unknown("gmail_lookup_unavailable");
            using var metadata = await GmailResponseJson.ReadAsync(detail, timeout.Token);
            return Matches(metadata.RootElement, identity, id) ? Success(id) : Unknown("gmail_lookup_unconfirmed");
        }
        catch (OperationCanceledException) { return Unknown("gmail_lookup_unavailable"); }
        catch (HttpRequestException) { return Unknown("gmail_lookup_unavailable"); }
        catch (IOException) { return Unknown("gmail_lookup_unavailable"); }
        catch (Exception exception) when (exception is JsonException or InvalidDataException)
        { return Unknown("gmail_response_invalid"); }
    }

    private static bool Matches(JsonElement root, string identity, string id) =>
        root.ValueKind == JsonValueKind.Object && GmailResponseJson.MessageId(root) == id &&
        root.TryGetProperty("labelIds", out var labels) && labels.ValueKind == JsonValueKind.Array &&
        labels.EnumerateArray().Any(x => x.ValueKind == JsonValueKind.String && x.GetString() == "SENT") &&
        root.TryGetProperty("payload", out var payload) && payload.ValueKind == JsonValueKind.Object && payload.TryGetProperty("headers", out var headers) && headers.ValueKind == JsonValueKind.Array &&
        headers.EnumerateArray().Any(header => header.ValueKind == JsonValueKind.Object && header.TryGetProperty("name", out var name) && name.ValueKind == JsonValueKind.String &&
            string.Equals(name.GetString(), "Message-ID", StringComparison.OrdinalIgnoreCase) &&
            header.TryGetProperty("value", out var value) && value.ValueKind == JsonValueKind.String && value.GetString()?.Trim() == identity);

    private static HttpRequestMessage Request(string uri, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    private static QuoteDeliveryProviderResult Unknown(string code) => new(QuoteDeliveryProviderOutcome.Unknown, null, code);
    private static QuoteDeliveryProviderResult Success(string id) => new(QuoteDeliveryProviderOutcome.ConfirmedSuccess, id, null);
}
