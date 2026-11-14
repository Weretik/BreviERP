using HostApiTests.TestSupport;
using BuildingBlocks.Api.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;

namespace HostApiTests;

/// <summary>
/// The HTTP surface of every module stays the same while controllers move to Minimal API: method, route with its
/// constraints, endpoint name (operationId), access rule, «Права доступу» action, body and form size limits of every endpoint are compared with
/// <c>Endpoints.snapshot.txt</c>. Set BREVIERP_UPDATE_ENDPOINTS=1 to rewrite the file after an intended change.
/// </summary>
public sealed class EndpointInventoryTests
{
    private static string SnapshotPath =>
        Path.Combine(Root(), "tests", "HostApiTests", "Endpoints.snapshot.txt");

    [Test]
    public async Task Endpoints_match_the_snapshot()
    {
        await using var host = await ApiTestHost.CreateAsync();
        var lines = host.ApiEndpoints().Select(Line).Order(StringComparer.Ordinal).ToArray();

        if (Environment.GetEnvironmentVariable("BREVIERP_UPDATE_ENDPOINTS") == "1")
            await File.WriteAllLinesAsync(SnapshotPath, lines);

        var expected = await File.ReadAllLinesAsync(SnapshotPath);
        Assert.Multiple(() =>
        {
            Assert.That(lines.Except(expected), Is.Empty, "new or changed endpoints");
            Assert.That(expected.Except(lines), Is.Empty, "missing endpoints");
        });
    }

    private static string Line(RouteEndpoint endpoint)
    {
        var method = endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.FirstOrDefault() ?? "ANY";
        // A group's own root ("") ends in a slash; routing matches it with or without one, as MVC did.
        var route = endpoint.RoutePattern.RawText!.Trim('/');
        var name = endpoint.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName
                   ?? endpoint.Metadata.GetMetadata<IRouteNameMetadata>()?.RouteName ?? "-";
        string access;
        if (endpoint.Metadata.GetMetadata<IAllowAnonymous>() is not null) access = "anonymous";
        else
        {
            var data = endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>();
            var rules = data.Select(x => x.Policy).OfType<string>().Select(x => "policy:" + x)
                .Concat(data.Select(x => x.Roles).OfType<string>().Where(x => x.Length > 0).Select(x => "roles:" + x))
                .Distinct().Order(StringComparer.Ordinal).ToArray();
            access = rules.Length > 0 ? string.Join(",", rules) : data.Count > 0 ? "signed-in" : "open";
        }

        // «Права доступу» action named on the endpoint (POST that edits) and raised body or form limits are part of access.
        if (endpoint.Metadata.GetMetadata<AccessActionAttribute>() is { } action) access += ",action:" + action.Action;
        if (endpoint.Metadata.GetMetadata<IRequestSizeLimitMetadata>() is { } limit)
            access += ",body-limit:" + (limit.MaxRequestBodySize?.ToString() ?? "none");
        if (endpoint.Metadata.GetMetadata<IFormOptionsMetadata>()?.MultipartBodyLengthLimit is { } form)
            access += ",form-limit:" + form;

        return $"{method} {route} | {name} | {access}";
    }

    private static string Root()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BreviERP.sln")))
            directory = directory.Parent;
        return directory!.FullName;
    }
}
