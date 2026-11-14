using System.Net;
using System.Net.Http.Headers;
using Ardalis.Result;
using BuildingBlocks.Api.Outbox;
using HostApiTests.TestSupport;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scheduling.Api.DependencyInjection;
using Scheduling.Api.Endpoints.Tick.RunSchedulerTick;
using Scheduling.Application.Features.Tick.Commands.RunSchedulerTick;

namespace HostApiTests;

/// <summary>SCH-001 R-10, SC-10: only Cloud Scheduler's token runs the tick.</summary>
public sealed class SchedulerTickEndpointTests
{
    private const string GoodToken = "scheduler-token";

    [Test]
    public async Task Without_configuration_the_tick_is_not_there()
    {
        var (host, recorder) = await Start(configured: false);
        using var _ = host;

        var response = await Post(host, GoodToken);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        Assert.That(recorder.Messages, Is.Empty);
    }

    [TestCase(null)]
    [TestCase("someone-else")]
    public async Task Another_caller_is_refused(string? token)
    {
        var (host, recorder) = await Start(configured: true);
        using var _ = host;

        var response = await Post(host, token);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        Assert.That(recorder.Messages, Is.Empty);
    }

    [Test]
    public async Task The_scheduler_runs_the_tick()
    {
        var (host, recorder) = await Start(configured: true);
        using var _ = host;

        var response = await Post(host, GoodToken);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(recorder.Messages.Single(), Is.TypeOf<RunSchedulerTickCommand>());
    }

    private static async Task<HttpResponseMessage> Post(IHost host, string? token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/internal/scheduler/tick");
        if (token is not null) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await host.GetTestClient().SendAsync(request);
    }

    private static async Task<(IHost Host, RecordingSender Recorder)> Start(bool configured)
    {
        var settings = configured
            ? new Dictionary<string, string?>
            {
                ["Scheduler:Audience"] = "https://api.example/internal/scheduler/tick",
                ["Scheduler:SchedulerServiceAccount"] = "scheduler@project.iam.gserviceaccount.com",
            }
            : new Dictionary<string, string?>();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        var (sender, recorder) = RecordingSender.Create(
            Result.Success(new SchedulerTickResponse(new Dictionary<string, string>(), [], null)));
        var host = new HostBuilder()
            .ConfigureWebHost(web => web.UseTestServer()
                .ConfigureServices(services =>
                {
                    services.AddRouting();
                    services.AddAuthorization();
                    services.AddSingleton<IOutboxCallerCheck, FakeCallerCheck>();
                    services.Configure<SchedulerTickOptions>(configuration.GetSection(SchedulerTickOptions.Section));
                    services.AddSingleton(sender);
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseAuthorization();
                    app.UseEndpoints(endpoints => endpoints.MapSchedulingEndpoints());
                }))
            .Build();
        await host.StartAsync();
        return (host, recorder);
    }

    private sealed class FakeCallerCheck : IOutboxCallerCheck
    {
        public Task<bool> IsSchedulerAsync(string token, OutboxDispatchOptions options) =>
            Task.FromResult(token == GoodToken && options.Audience!.EndsWith("/internal/scheduler/tick", StringComparison.Ordinal));
    }
}
