using System.Net;
using System.Net.Http.Headers;
using BuildingBlocks.Api.Outbox;
using BuildingBlocks.Application.Outbox;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HostApiTests;

/// <summary>PLT-003 R-06, SC-07: only the scheduler's token runs the outbox.</summary>
public sealed class OutboxDispatchEndpointTests
{
    private const string GoodToken = "scheduler-token";

    [Test]
    public async Task Without_configuration_the_endpoint_is_not_there()
    {
        using var host = await Start(configured: false);

        var response = await Post(host, GoodToken);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [TestCase(null)]
    [TestCase("someone-else")]
    public async Task Another_caller_is_refused(string? token)
    {
        using var host = await Start(configured: true);

        var response = await Post(host, token);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(host.Services.GetRequiredService<CountingDispatcher>().Runs, Is.Zero);
        });
    }

    [Test]
    public async Task The_scheduler_runs_the_outbox()
    {
        using var host = await Start(configured: true);

        var response = await Post(host, GoodToken);

        Assert.Multiple(async () =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(await response.Content.ReadAsStringAsync(), Is.EqualTo("""{"processed":2,"failed":1}"""));
            Assert.That(host.Services.GetRequiredService<CountingDispatcher>().Runs, Is.EqualTo(1));
        });
    }

    private static async Task<HttpResponseMessage> Post(IHost host, string? token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, OutboxDispatchEndpoint.Route);
        if (token is not null) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await host.GetTestClient().SendAsync(request);
    }

    private static async Task<IHost> Start(bool configured)
    {
        var settings = configured
            ? new Dictionary<string, string?>
            {
                ["Outbox:Audience"] = "https://api.example/internal/outbox/dispatch",
                ["Outbox:SchedulerServiceAccount"] = "outbox@project.iam.gserviceaccount.com",
            }
            : new Dictionary<string, string?>();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        var host = new HostBuilder()
            .ConfigureWebHost(web => web.UseTestServer()
                .ConfigureServices(services =>
                {
                    services.AddRouting();
                    services.AddAuthorization();
                    services.AddSingleton<IOutboxCallerCheck, FakeCallerCheck>();
                    services.AddOutboxDispatchEndpoint(configuration);
                    services.AddSingleton<CountingDispatcher>();
                    services.AddSingleton<IOutboxDispatcher>(x => x.GetRequiredService<CountingDispatcher>());
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseAuthorization();
                    app.UseEndpoints(endpoints => endpoints.MapOutboxDispatch());
                }))
            .Build();
        await host.StartAsync();
        return host;
    }

    private sealed class FakeCallerCheck : IOutboxCallerCheck
    {
        public Task<bool> IsSchedulerAsync(string token, OutboxDispatchOptions options) =>
            Task.FromResult(token == GoodToken);
    }

    private sealed class CountingDispatcher : IOutboxDispatcher
    {
        public int Runs { get; private set; }

        public Task DispatchAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<OutboxRun> DispatchDueAsync(CancellationToken cancellationToken)
        {
            Runs++;
            return Task.FromResult(new OutboxRun(2, 1));
        }
    }
}
