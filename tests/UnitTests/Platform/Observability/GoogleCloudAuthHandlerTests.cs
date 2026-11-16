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

    private sealed class RecordingHandler : HttpMessageHandler
    {
        public AuthenticationHeaderValue? Authorization { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Authorization = request.Headers.Authorization;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
