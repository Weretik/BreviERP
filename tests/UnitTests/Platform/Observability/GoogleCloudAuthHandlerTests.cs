using System.Net;
using System.Net.Http.Headers;
using BuildingBlocks.Observability;

namespace UnitTests.Platform.Observability;

/// <summary>PLT-004 R-002: the exporter calls the Telemetry API with the runtime account's token.</summary>
public sealed class GoogleCloudAuthHandlerTests
{
    [Test]
    public async Task A_request_carries_the_runtime_account_token()
    {
        var inner = new RecordingHandler();
        using var client = new HttpClient(new GoogleCloudAuthHandler(_ => Task.FromResult("token-1")) { InnerHandler = inner });

        await client.PostAsync("https://telemetry.googleapis.com/v1/traces", new ByteArrayContent([]));

        Assert.Multiple(() =>
        {
            Assert.That(inner.Authorization?.Scheme, Is.EqualTo("Bearer"));
            Assert.That(inner.Authorization?.Parameter, Is.EqualTo("token-1"));
        });
    }

    [Test]
    public void A_synchronous_request_carries_the_token_too_because_the_otlp_exporter_sends_synchronously()
    {
        var inner = new RecordingHandler();
        using var client = new HttpClient(new GoogleCloudAuthHandler(_ => Task.FromResult("token-2")) { InnerHandler = inner });

        client.Send(new HttpRequestMessage(HttpMethod.Post, "https://telemetry.googleapis.com/v1/traces") { Content = new ByteArrayContent([]) });

        Assert.Multiple(() =>
        {
            Assert.That(inner.Authorization?.Scheme, Is.EqualTo("Bearer"));
            Assert.That(inner.Authorization?.Parameter, Is.EqualTo("token-2"));
        });
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        public AuthenticationHeaderValue? Authorization { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Authorization = request.Headers.Authorization;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }

        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Authorization = request.Headers.Authorization;
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
