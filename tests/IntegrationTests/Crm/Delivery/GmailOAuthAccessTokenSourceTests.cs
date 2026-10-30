using System.Net;
using System.Text;
using Crm.Infrastructure.Delivery.Gmail.OAuth;
using Crm.Infrastructure.Delivery.Readiness;

namespace IntegrationTests.Crm.Delivery;

public sealed class GmailOAuthAccessTokenSourceTests
{
    private const string Sender = "sender@example.test";
    private const string Scopes = "https://www.googleapis.com/auth/gmail.send https://www.googleapis.com/auth/gmail.readonly";
    private static string Token(string access = "fixture-access", string? scopes = Scopes, int expires = 3600)
        => System.Text.Json.JsonSerializer.Serialize(new { access_token = access, token_type = "Bearer", expires_in = expires, scope = scopes });
    private static HttpResponseMessage Json(string body, HttpStatusCode status = HttpStatusCode.OK)
        => new(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") };

    [Test]
    public async Task Refresh_verifies_same_account_and_caches_only_validated_token()
    {
        var http = new FakeHttp(index => Json(index % 2 == 0 ? Token() : "{\"emailAddress\":\"" + Sender + "\"}"));
        var source = Source(http);
        Assert.That(await source.GetAccessTokenAsync(default), Is.EqualTo("fixture-access"));
        Assert.That(await source.GetAccessTokenAsync(default), Is.EqualTo("fixture-access"));
        Assert.That(http.Calls, Has.Count.EqualTo(2));
        Assert.That(http.Calls[0].Method, Is.EqualTo(HttpMethod.Post));
        Assert.That(http.Calls[0].Uri, Is.EqualTo("https://oauth2.googleapis.com/token"));
        Assert.That(http.Calls[0].Body, Does.Contain("grant_type=refresh_token"));
        Assert.That(http.Calls[1].Uri, Is.EqualTo("https://gmail.googleapis.com/gmail/v1/users/me/profile"));
        Assert.That(http.Calls[1].Authorization, Is.EqualTo("Bearer fixture-access"));
    }

    [Test]
    public async Task Concurrent_callers_share_one_refresh_and_profile_check()
    {
        var http = new FakeHttp(index => Json(index == 0 ? Token() : "{\"emailAddress\":\"" + Sender + "\"}"));
        var source = Source(http);
        var tokens = await Task.WhenAll(Enumerable.Range(0, 12).Select(_ => source.GetAccessTokenAsync(default).AsTask()));
        Assert.That(tokens.All(x => x == "fixture-access"), Is.True);
        Assert.That(http.Calls, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task Expired_cache_refreshes_and_rechecks_account()
    {
        var time = new ManualTime();
        var http = new FakeHttp(index => Json(index % 2 == 0 ? Token("access-" + index) : "{\"emailAddress\":\"" + Sender + "\"}"));
        var source = Source(http, time: time);
        Assert.That(await source.GetAccessTokenAsync(default), Is.EqualTo("access-0"));
        time.Now += TimeSpan.FromSeconds(3540);
        Assert.That(await source.GetAccessTokenAsync(default), Is.EqualTo("access-2"));
        Assert.That(http.Calls, Has.Count.EqualTo(4));
    }

    [TestCase("other@example.test")]
    [TestCase("")]
    public async Task Wrong_or_missing_profile_never_returns_token(string email)
    {
        var http = new FakeHttp(index => Json(index == 0 ? Token() : "{\"emailAddress\":\"" + email + "\"}"));
        Assert.That(await Source(http).GetAccessTokenAsync(default), Is.Null);
        Assert.That(http.Calls, Has.Count.EqualTo(2));
    }

    [TestCase("[]")]
    [TestCase("{\"access_token\":\"fixture\",\"token_type\":\"Basic\",\"expires_in\":3600}")]
    [TestCase("{\"access_token\":\"fixture\",\"token_type\":\"Bearer\",\"expires_in\":20}")]
    public async Task Invalid_refresh_payload_is_unavailable_without_profile_or_retry(string response)
    {
        var http = new FakeHttp(_ => Json(response));
        Assert.That(await Source(http).GetAccessTokenAsync(default), Is.Null);
        Assert.That(http.Calls, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Explicit_insufficient_scopes_are_rejected()
    {
        var http = new FakeHttp(_ => Json(Token(scopes: "https://www.googleapis.com/auth/gmail.send")));
        Assert.That(await Source(http).GetAccessTokenAsync(default), Is.Null);
        Assert.That(http.Calls, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Omitted_scope_preserves_original_grant_and_still_requires_profile()
    {
        var http = new FakeHttp(index => Json(index == 0 ? "{\"access_token\":\"fixture-access\",\"token_type\":\"Bearer\",\"expires_in\":3600}" : "{\"emailAddress\":\"" + Sender + "\"}"));
        Assert.That(await Source(http).GetAccessTokenAsync(default), Is.EqualTo("fixture-access"));
    }

    [Test]
    public async Task Missing_configuration_and_disabled_delivery_never_reach_network()
    {
        var http = new FakeHttp(_ => throw new AssertionException("Unexpected HTTP."));
        Assert.That(await Source(http, options: new()).GetAccessTokenAsync(default), Is.Null);
        Assert.That(await Source(http, enabled: false).GetAccessTokenAsync(default), Is.Null);
        Assert.That(http.Calls, Is.Empty);
    }

    [TestCase(400)]
    [TestCase(429)]
    [TestCase(503)]
    public async Task Rejected_refresh_is_unavailable_without_retry(int status)
    {
        var http = new FakeHttp(_ => Json("{\"error\":\"fixture-sensitive-error\"}", (HttpStatusCode)status));
        Assert.That(await Source(http).GetAccessTokenAsync(default), Is.Null);
        Assert.That(http.Calls, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Caller_cancellation_propagates_before_network()
    {
        var http = new FakeHttp(_ => throw new AssertionException("Unexpected HTTP."));
        using var cancel = new CancellationTokenSource();
        cancel.Cancel();
        Assert.ThrowsAsync<OperationCanceledException>(async () => await Source(http).GetAccessTokenAsync(cancel.Token));
        Assert.That(http.Calls, Is.Empty);
        await Task.CompletedTask;
    }

    [Test]
    public async Task Interrupted_transport_is_unavailable_without_retry()
    {
        var http = new FakeHttp(_ => throw new HttpRequestException("Fixture transport unavailable."));
        Assert.That(await Source(http).GetAccessTokenAsync(default), Is.Null);
        Assert.That(http.Calls, Has.Count.EqualTo(1));
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Interrupted_response_body_is_unavailable_without_retry(bool profile)
    {
        var http = new FakeHttp(index => profile && index == 0 ? Json(Token()) :
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(new BrokenStream()) });
        Assert.That(await Source(http).GetAccessTokenAsync(default), Is.Null);
        Assert.That(http.Calls, Has.Count.EqualTo(profile ? 2 : 1));
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Oversized_refresh_or_profile_response_is_unavailable(bool profile)
    {
        var http = new FakeHttp(index => Json(profile && index == 0 ? Token() : new string('x', 16385)));
        Assert.That(await Source(http).GetAccessTokenAsync(default), Is.Null);
        Assert.That(http.Calls, Has.Count.EqualTo(profile ? 2 : 1));
    }

    [Test]
    public async Task String_expiry_is_invalid_instead_of_throwing()
    {
        var http = new FakeHttp(_ => Json("{\"access_token\":\"fixture\",\"token_type\":\"Bearer\",\"expires_in\":\"3600\"}"));
        Assert.That(await Source(http).GetAccessTokenAsync(default), Is.Null);
        Assert.That(http.Calls, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Provider_timeout_is_unavailable_and_does_not_retry()
    {
        var http = new FakeHttp(_ => throw new TaskCanceledException());
        Assert.That(await Source(http).GetAccessTokenAsync(default), Is.Null);
        Assert.That(http.Calls, Has.Count.EqualTo(1));
    }

    private sealed class BrokenStream() : MemoryStream(new byte[1])
    {
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            => ValueTask.FromException<int>(new IOException("Fixture interrupted body."));
    }

    private static GmailOAuthAccessTokenSource Source(FakeHttp http, GmailOAuthOptions? options = null,
        bool enabled = true, TimeProvider? time = null) => new(new HttpClient(http), options ?? new GmailOAuthOptions {
            ClientId = "fixture-client", ClientSecret = "fixture-client-secret", RefreshToken = "fixture-refresh" },
            new QuoteDeliveryProviderOptions { Enabled = enabled, ProviderKey = "gmail", SenderAddress = Sender }, time);

    private sealed class ManualTime : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = new(2026, 10, 30, 12, 0, 0, TimeSpan.Zero);
        public override DateTimeOffset GetUtcNow() => Now;
    }

    private sealed record Call(HttpMethod Method, string Uri, string? Body, string? Authorization);
    private sealed class FakeHttp(Func<int, HttpResponseMessage> response) : HttpMessageHandler
    {
        public List<Call> Calls { get; } = [];
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var index = Calls.Count;
            Calls.Add(new Call(request.Method, request.RequestUri!.AbsoluteUri,
                request.Content is null ? null : await request.Content.ReadAsStringAsync(ct), request.Headers.Authorization?.ToString()));
            await Task.Yield();
            return response(index);
        }
    }
}
