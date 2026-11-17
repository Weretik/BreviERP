using System.Net;
using Host.Api.DependencyInjection.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;

namespace HostApiTests.Http;

/// <summary>
/// `/health` answers without the database; `/health/ready` checks PostgreSQL, so an instance without its database is
/// reported as not ready (reliability REL-04).
/// </summary>
public sealed class HealthEndpointsTests
{
    // Nothing listens on port 1: the connection is refused at once.
    private const string UnreachableDatabase = "Host=127.0.0.1;Port=1;Database=none;Username=none;Password=none;Timeout=2";

    [Test]
    public async Task Liveness_answers_without_the_database()
    {
        await using var app = await StartAsync(UnreachableDatabase);

        using var response = await app.GetTestClient().GetAsync("/health");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [TestCase(UnreachableDatabase)]
    [TestCase(null)]
    public async Task Readiness_is_unavailable_without_a_reachable_database(string? connectionString)
    {
        await using var app = await StartAsync(connectionString);

        using var response = await app.GetTestClient().GetAsync("/health/ready");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
        Assert.That(await response.Content.ReadAsStringAsync(), Is.EqualTo("Unhealthy"));
    }

    private static async Task<WebApplication> StartAsync(string? connectionString)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Production" });
        builder.WebHost.UseTestServer();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Default"] = connectionString,
        });
        builder.Services.AddHealthEndpoints();
        var app = builder.Build();
        app.MapHealthEndpoints();
        await app.StartAsync();
        return app;
    }
}
