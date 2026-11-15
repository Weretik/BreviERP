using System.Text.Json.Nodes;

namespace HostApiTests.Http;

/// <summary>
/// The build writes the OpenAPI document of the code to <c>docs/sdd/contracts/openapi.json</c> (ADR-0015); CI fails when
/// the committed file differs from it. These rules keep that generated contract usable by the typed frontend clients.
/// </summary>
public sealed class OpenApiDocumentRulesTests
{
    private static readonly string[] Methods = ["get", "post", "put", "patch", "delete"];

    /// <summary>Reads whose answer is a file, a redirect or always an error: they have no JSON body to type.</summary>
    private static readonly string[] BodilessReads =
    [
        "getFinanceAttachmentContent", "getFinanceExport", "getFinanceReportCsv", "getCrmQuotePdf",
        "crmMailboxGoogleCallback",
    ];

    [Test]
    public void Every_operation_has_a_unique_operation_id_a_summary_and_a_tag()
    {
        var operations = Operations().ToList();
        var missing = operations
            .Where(op => op.Operation["operationId"] is null || op.Operation["summary"] is null || op.Operation["tags"] is null)
            .Select(op => op.Name);
        var duplicates = operations
            .Select(op => op.Operation["operationId"]?.GetValue<string>())
            .OfType<string>()
            .GroupBy(id => id)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key);

        Assert.Multiple(() =>
        {
            Assert.That(operations, Is.Not.Empty);
            Assert.That(missing.Concat(duplicates), Is.Empty);
        });
    }

    [Test]
    public void Every_read_declares_its_success_body()
    {
        // A read without Produces<T> reaches the frontends as `unknown`.
        var untyped = Operations()
            .Where(op => op.Name.StartsWith("GET ", StringComparison.Ordinal))
            .Where(op => !BodilessReads.Contains(op.Operation["operationId"]?.GetValue<string>()))
            .Where(op => (op.Operation["responses"] as JsonObject ?? [])
                .Where(response => response.Key[0] == '2')
                .All(response => response.Value?["content"] is null))
            .Select(op => op.Name);

        Assert.That(untyped, Is.Empty);
    }

    [Test]
    public void Query_parameters_are_camel_case()
    {
        var violations = QueryParameterNames().Where(name => !char.IsLower(name[0])).ToList();

        Assert.That(violations, Is.Empty);
    }

    [Test]
    public void Numbers_are_not_also_strings()
    {
        // The web JSON defaults read "12" as 12, which the generator documents as integer|string with a pattern; the
        // contract promises numbers, so clients get number types.
        var violations = Nodes(Document())
            .OfType<JsonObject>()
            .Where(node => node["type"] is JsonArray types
                && types.Any(type => type?.GetValue<string>() == "string")
                && types.Any(type => type?.GetValue<string>() is "integer" or "number"))
            .Select(node => node.GetPath());

        Assert.That(violations, Is.Empty);
    }

    private static JsonNode Document()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BreviERP.sln")))
            directory = directory.Parent;
        var path = Path.Combine(directory!.FullName, "docs", "sdd", "contracts", "openapi.json");
        return JsonNode.Parse(File.ReadAllText(path)) ?? throw new InvalidOperationException($"{path} is empty.");
    }

    private static IEnumerable<(string Name, JsonObject Operation)> Operations() =>
        (Document()["paths"] as JsonObject ?? [])
        .SelectMany(path => Methods
            .Where(method => path.Value?[method] is JsonObject)
            .Select(method => ($"{method.ToUpperInvariant()} {path.Key}", (JsonObject)path.Value![method]!)));

    private static IEnumerable<string> QueryParameterNames() =>
        Operations()
            .SelectMany(op => op.Operation["parameters"] as JsonArray ?? [])
            .Where(parameter => parameter?["in"]?.GetValue<string>() == "query")
            .Select(parameter => parameter!["name"]!.GetValue<string>());

    private static IEnumerable<JsonNode> Nodes(JsonNode? node) => node switch
    {
        JsonObject obj => obj.SelectMany(property => Nodes(property.Value)).Prepend(obj),
        JsonArray array => array.SelectMany(Nodes).Prepend(array),
        null => [],
        _ => [node],
    };
}
