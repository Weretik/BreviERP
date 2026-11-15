using System.Text.Json.Nodes;

namespace UnitTests.TestSupport;

/// <summary>
/// The OpenAPI document the Host.Api build generates from the code: <c>docs/sdd/contracts/openapi.json</c> (ADR-0015).
/// Tests use it to check that an endpoint is published in the contract, not hidden from it.
/// </summary>
internal static class ApiContract
{
    private static readonly string[] Methods = ["get", "post", "put", "patch", "delete"];
    private static readonly Lazy<JsonNode> Document = new(Load);

    public static IReadOnlyCollection<string> OperationIds =>
        (Document.Value["paths"] as JsonObject ?? [])
        .SelectMany(path => Methods.Select(method => path.Value?[method]?["operationId"]?.GetValue<string>()))
        .OfType<string>()
        .ToArray();

    public static IReadOnlyCollection<string> Paths =>
        (Document.Value["paths"] as JsonObject ?? []).Select(path => path.Key).ToArray();

    /// <summary>The names a C# enum schema lists; empty when the contract has no such enum.</summary>
    public static IReadOnlyCollection<string> EnumValues(string schema) =>
        (Document.Value["components"]?["schemas"]?[schema]?["enum"] as JsonArray ?? [])
        .Select(value => value!.GetValue<string>())
        .ToArray();

    private static JsonNode Load()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BreviERP.sln")))
            directory = directory.Parent;
        var path = Path.Combine(directory!.FullName, "docs", "sdd", "contracts", "openapi.json");
        return JsonNode.Parse(File.ReadAllText(path)) ?? throw new InvalidOperationException($"{path} is empty.");
    }
}
