using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using BuildingBlocks.Api.Http;

namespace HostApiTests;

/// <summary>Query enums bind by name in any letter case or by number, as they did in the controllers.</summary>
public sealed class QueryEnumTests
{
    public enum Kind { All, NeedsReview }

    [TestCase("needsReview", "NeedsReview")]
    [TestCase("NEEDSREVIEW", "NeedsReview")]
    [TestCase("1", "NeedsReview")]
    [TestCase(null, "All")]
    public async Task Binds_like_mvc(string? value, string expected)
    {
        await using var app = await StartAsync();
        var response = await app.GetTestClient().GetAsync(value is null ? "/e" : "/e?kind=" + value);
        Assert.That(await response.Content.ReadAsStringAsync(), Is.EqualTo(expected));
    }

    [Test]
    public async Task An_unknown_name_is_400()
    {
        await using var app = await StartAsync();
        var response = await app.GetTestClient().GetAsync("/e?kind=whatever");
        Assert.That((int)response.StatusCode, Is.EqualTo(400));
    }

    private static async Task<WebApplication> StartAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        var app = builder.Build();
        app.MapGet("/e", (QueryEnum<Kind>? kind) => (kind?.Value ?? Kind.All).ToString());
        await app.StartAsync();
        return app;
    }
}
