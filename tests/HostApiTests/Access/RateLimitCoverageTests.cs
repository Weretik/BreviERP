using System.Text.RegularExpressions;
using HostApiTests.TestSupport;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;

namespace HostApiTests.Access;

/// <summary>
/// Rate limits are decided with every endpoint, not afterwards (docs: security/rate-limiting.md): a public endpoint names
/// a policy, a policy name is one the host registers, and uploads and heavy operations use their own policy. The global
/// soft limit covers everything else.
/// </summary>
public sealed class RateLimitCoverageTests
{
    private static readonly string[] Registered =
        ["AuthLogin", "SiteRequest", "PublicRead", "PublicSearch", "Upload", "HeavyOperation"];

    // Telegram calls the webhook (secret token) and Cloud Scheduler the tick (shared secret); neither is a browser.
    private static readonly string[] Unnamed = ["api/reminders/telegram/webhook", "internal/scheduler/tick"];

    [Test]
    public async Task Every_anonymous_endpoint_names_a_rate_limit_policy()
    {
        await using var host = await ApiTestHost.CreateAsync();

        var violations = host.ApiEndpoints()
            .Where(endpoint => endpoint.Metadata.GetMetadata<IAllowAnonymous>() is not null)
            .Where(endpoint => endpoint.Metadata.GetMetadata<EnableRateLimitingAttribute>() is null)
            .Select(endpoint => ApiTestHost.Describe(endpoint))
            .Where(line => !Unnamed.Any(route => line.EndsWith(route, StringComparison.Ordinal)))
            .Order()
            .ToArray();

        Assert.That(violations, Is.Empty);
    }

    [Test]
    public async Task Every_named_policy_is_registered_by_the_host()
    {
        await using var host = await ApiTestHost.CreateAsync();

        var violations = host.ApiEndpoints()
            .Select(endpoint => (Line: ApiTestHost.Describe(endpoint),
                Policy: endpoint.Metadata.GetMetadata<EnableRateLimitingAttribute>()?.PolicyName))
            .Where(item => item.Policy is not null && !Registered.Contains(item.Policy))
            .Select(item => $"{item.Line}: {item.Policy}")
            .Order()
            .ToArray();

        Assert.That(violations, Is.Empty);
    }

    [Test]
    public async Task Uploads_and_heavy_operations_use_their_own_policy()
    {
        await using var host = await ApiTestHost.CreateAsync();

        var violations = host.ApiEndpoints()
            .Select(endpoint => (Line: ApiTestHost.Describe(endpoint),
                Name: endpoint.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName ?? "",
                Policy: endpoint.Metadata.GetMetadata<EnableRateLimitingAttribute>()?.PolicyName))
            .Select(item => (item.Line, Expected: ExpectedPolicy(item.Name), item.Policy))
            .Where(item => item.Expected is not null && item.Policy != item.Expected)
            .Select(item => $"{item.Line}: expected {item.Expected}")
            .Order()
            .ToArray();

        Assert.That(violations, Is.Empty);
    }

    private static string? ExpectedPolicy(string operationId) =>
        operationId.StartsWith("upload", StringComparison.Ordinal) ? "Upload"
        : Regex.IsMatch(operationId, "^(import|sync)|Import|Pdf|Export|Report") ? "HeavyOperation"
        : null;
}
