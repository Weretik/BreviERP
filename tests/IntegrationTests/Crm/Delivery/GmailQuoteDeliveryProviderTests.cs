using System.Net;
using System.Text;
using System.Text.Json;
using Crm.Application.Delivery.Provider;
using Crm.Infrastructure.Delivery.Gmail;
using Crm.Infrastructure.Delivery.Readiness;

namespace IntegrationTests.Crm.Delivery;

public sealed class GmailQuoteDeliveryProviderTests
{
    private static readonly Guid DeliveryId = Guid.Parse("b0eb4b07-6d1a-4390-b77e-53f113c3f859");
    private static string Identity => $"brevi-v1-{DeliveryId:N}@example.test";
    private static QuoteDeliverySubmission Submission => new(DeliveryId,
        new DateTimeOffset(2026, 10, 30, 10, 0, 0, TimeSpan.Zero),
        "sandbox@example.test", "КП для замовника", "Публічний текст листа", "%PDF-1.7\nfixture"u8.ToArray());

    [Test]
    public async Task Submit_uses_fixed_Gmail_endpoint_stable_identity_and_public_MIME_PDF()
    {
        var handler = new FakeHttp(_ => Json(HttpStatusCode.OK, "{\"id\":\"msg123\"}"));
        var tokens = new FakeToken("fixture-token");
        var result = await Provider(handler, tokens).SubmitAsync(Submission, default);
        Assert.That(result.Outcome, Is.EqualTo(QuoteDeliveryProviderOutcome.ConfirmedSuccess));
        Assert.That(result.ProviderMessageId, Is.EqualTo("msg123"));
        Assert.That(handler.Calls, Has.Count.EqualTo(1));
        var call = handler.Calls[0];
        Assert.That(call.Method, Is.EqualTo(HttpMethod.Post));
        Assert.That(call.Uri, Is.EqualTo("https://gmail.googleapis.com/gmail/v1/users/me/messages/send"));
        using var body = JsonDocument.Parse(call.Body!);
        var raw = body.RootElement.GetProperty("raw").GetString()!;
        raw = raw.Replace('-', '+').Replace('_', '/').PadRight((raw.Length + 3) / 4 * 4, '=');
        var mime = Encoding.UTF8.GetString(Convert.FromBase64String(raw));
        Assert.That(mime, Does.Contain("From: sender@example.test\r\nTo: sandbox@example.test\r\n"));
        Assert.That(mime, Does.Contain("Message-ID: <" + Identity + ">\r\n"));
        Assert.That(mime, Does.Contain("Subject: =?utf-8?B?"));
        Assert.That(mime, Does.Contain("Content-Type: application/pdf; name=quote.pdf"));
        Assert.That(mime, Does.Contain(Convert.ToBase64String(Submission.PdfBytes.Span)));
        Assert.That(mime, Does.Contain(Convert.ToBase64String(Encoding.UTF8.GetBytes(Submission.PlainText))));
        Assert.That(mime, Does.Not.Contain("fixture-token"));
    }

    [TestCase(500)]
    [TestCase(429)]
    [TestCase(408)]
    public async Task Unknown_send_response_is_not_retried(int status)
    {
        var handler = new FakeHttp(_ => Json((HttpStatusCode)status, "{}"));
        var result = await Provider(handler).SubmitAsync(Submission, default);
        Assert.That(result.Outcome, Is.EqualTo(QuoteDeliveryProviderOutcome.Unknown));
        Assert.That(handler.Calls, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Transport_timeout_is_unknown_without_resend()
    {
        var handler = new FakeHttp(_ => throw new TaskCanceledException());
        var result = await Provider(handler).SubmitAsync(Submission, default);
        Assert.That(result.Outcome, Is.EqualTo(QuoteDeliveryProviderOutcome.Unknown));
        Assert.That(handler.Calls, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Send_only_unknown_requires_manual_review_without_token_or_HTTP()
    {
        var handler = new FakeHttp(_ => throw new AssertionException("Mail must not be read."));
        var tokens = new FakeToken("fixture-token");
        var provider = Provider(handler, tokens);
        Assert.That(provider.Capabilities.SupportsLookupBySendKey, Is.False);
        var result = await provider.ReconcileAsync(DeliveryId, default);
        Assert.That(result.Outcome, Is.EqualTo(QuoteDeliveryProviderOutcome.Unknown));
        Assert.That(result.ErrorCode, Is.EqualTo("delivery_manual_review_required"));
        Assert.That(tokens.Calls, Is.Zero);
        Assert.That(handler.Calls, Is.Empty);
    }

    [Test]
    public async Task Disabled_missing_token_and_non_sandbox_target_never_reach_HTTP()
    {
        var handler = new FakeHttp(_ => throw new AssertionException("Unexpected network attempt"));
        var tokens = new FakeToken(null);
        var disabled = await Provider(handler, tokens, enabled: false).SubmitAsync(Submission, default);
        Assert.That(disabled.ErrorCode, Is.EqualTo("delivery_disabled"));
        Assert.That(tokens.Calls, Is.Zero);
        var unavailable = await Provider(handler, tokens).SubmitAsync(Submission, default);
        Assert.That(unavailable.ErrorCode, Is.EqualTo("gmail_oauth_unavailable"));
        var blocked = await Provider(handler, tokens).SubmitAsync(Submission with { RecipientAddress = "customer@example.test" }, default);
        Assert.That(blocked.ErrorCode, Is.EqualTo("delivery_sandbox_recipient_required"));
        Assert.That(tokens.Calls, Is.EqualTo(1));
        Assert.That(handler.Calls, Is.Empty);
    }

    [Test]
    public async Task Header_injection_is_rejected_before_token_or_HTTP()
    {
        var handler = new FakeHttp(_ => throw new AssertionException("Unexpected network attempt"));
        var tokens = new FakeToken("fixture-token");
        var result = await Provider(handler, tokens).SubmitAsync(Submission with { Subject = "Quote\r\nBcc: other@example.test" }, default);
        Assert.That(result.Outcome, Is.EqualTo(QuoteDeliveryProviderOutcome.ConfirmedFailure));
        Assert.That(result.ErrorCode, Is.EqualTo("gmail_message_invalid"));
        Assert.That(tokens.Calls, Is.Zero);
        Assert.That(handler.Calls, Is.Empty);
    }

    [TestCase("[]")]
    [TestCase("{\"id\":17}")]
    [TestCase("{\"id\":\"\"}")]
    public async Task Malformed_success_response_remains_unknown(string json)
    {
        var handler = new FakeHttp(_ => Json(HttpStatusCode.OK, json));
        var result = await Provider(handler).SubmitAsync(Submission, default);
        Assert.That(result.Outcome, Is.EqualTo(QuoteDeliveryProviderOutcome.Unknown));
        Assert.That(handler.Calls, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Interrupted_response_body_is_unknown_without_retry()
    {
        var handler = new FakeHttp(_ => new HttpResponseMessage(HttpStatusCode.OK)
            { Content = new StreamContent(new InterruptedResponseStream()) });
        var provider = Provider(handler);
        var result = await provider.SubmitAsync(Submission, default);
        Assert.That(result.Outcome, Is.EqualTo(QuoteDeliveryProviderOutcome.Unknown));
        Assert.That(handler.Calls, Has.Count.EqualTo(1));
        Assert.That(handler.Calls[0].Method, Is.EqualTo(HttpMethod.Post));
    }

    private sealed class InterruptedResponseStream() : MemoryStream(new byte[1])
    {
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            => ValueTask.FromException<int>(new IOException("Interrupted response body fixture."));
    }

    private static GmailQuoteDeliveryProvider Provider(FakeHttp handler, FakeToken? tokens = null, bool enabled = true) =>
        new(new HttpClient(handler) { BaseAddress = new Uri("https://unused.invalid") }, tokens ?? new FakeToken("fixture-token"),
            new QuoteDeliveryProviderOptions { Enabled = enabled, ProviderKey = "gmail", SenderAddress = "sender@example.test",
                SandboxOnly = true, TestRecipientAddress = "sandbox@example.test" });

    private static HttpResponseMessage Json(HttpStatusCode status, string json) => new(status)
        { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    private sealed class FakeToken(string? token) : IQuoteDeliveryAccessTokenSource
    {
        public int Calls { get; private set; }
        public ValueTask<string?> GetAccessTokenAsync(CancellationToken cancellationToken)
        { Calls++; return ValueTask.FromResult(token); }
    }

    private sealed record Call(HttpMethod Method, string Uri, string? Body);
    private sealed class FakeHttp(Func<HttpRequestMessage, HttpResponseMessage> response) : HttpMessageHandler
    {
        public List<Call> Calls { get; } = [];
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls.Add(new Call(request.Method, request.RequestUri!.AbsoluteUri,
                request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken)));
            return response(request);
        }
    }
}
