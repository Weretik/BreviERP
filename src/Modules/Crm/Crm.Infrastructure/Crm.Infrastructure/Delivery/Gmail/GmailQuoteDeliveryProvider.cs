using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Crm.Application.Delivery.Provider;
using Crm.Application.Delivery.Readiness;
using Crm.Infrastructure.Delivery.Readiness;

namespace Crm.Infrastructure.Delivery.Gmail;

/// <summary>One-attempt Gmail transport. Message-ID lookup correlates outcomes; empty lookup never permits resend.</summary>
public sealed class GmailQuoteDeliveryProvider(HttpClient http, IQuoteDeliveryAccessTokenSource tokens,
    QuoteDeliveryProviderOptions options) : IQuoteDeliveryProvider
{
    private const string Root = "https://gmail.googleapis.com/gmail/v1/users/me/messages";
    public QuoteDeliveryProviderCapabilities Capabilities { get; } = new("gmail", true, true);

    public async Task<QuoteDeliveryProviderResult> SubmitAsync(QuoteDeliverySubmission submission, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var readiness = new QuoteDeliveryReadinessPolicy(options, Capabilities, true).Assess(submission.RecipientAddress);
        if (!readiness.CanSubmit) return Failed(readiness.Blockers[0]);
        string raw;
        try { raw = GmailMimeMessage.Encode(submission, options.SenderAddress); }
        catch (ArgumentException) { return Failed("gmail_message_invalid"); }
        var token = await tokens.GetAccessTokenAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(token)) return Failed("gmail_oauth_unavailable");
        using var request = new HttpRequestMessage(HttpMethod.Post, Root + "/send");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(new { raw });
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(options.TimeoutSeconds));
        try
        {
            using var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
            if (!response.IsSuccessStatusCode) return Rejected(response.StatusCode);
            using var json = await GmailResponseJson.ReadAsync(response, timeout.Token);
            var id = GmailResponseJson.MessageId(json.RootElement);
            return id is null ? Unknown("gmail_response_invalid") : Success(id);
        }
        catch (OperationCanceledException) { return Unknown("gmail_send_outcome_unknown"); }
        catch (HttpRequestException) { return Unknown("gmail_send_outcome_unknown"); }
        catch (IOException) { return Unknown("gmail_send_outcome_unknown"); }
        catch (Exception exception) when (exception is JsonException or InvalidDataException)
        { return Unknown("gmail_response_invalid"); }
    }

    public Task<QuoteDeliveryProviderResult> ReconcileAsync(Guid deliveryId, CancellationToken cancellationToken)
        => new GmailSentMessageLookup(http, tokens, options).ReconcileAsync(deliveryId, cancellationToken);

    private static QuoteDeliveryProviderResult Rejected(HttpStatusCode status) =>
        (int)status >= 500 || status is HttpStatusCode.TooManyRequests or HttpStatusCode.RequestTimeout
            ? Unknown("gmail_send_outcome_unknown") : Failed("gmail_request_rejected");
    private static QuoteDeliveryProviderResult Failed(string code) => new(QuoteDeliveryProviderOutcome.ConfirmedFailure, null, code);
    private static QuoteDeliveryProviderResult Unknown(string code) => new(QuoteDeliveryProviderOutcome.Unknown, null, code);
    private static QuoteDeliveryProviderResult Success(string id) => new(QuoteDeliveryProviderOutcome.ConfirmedSuccess, id, null);
}
