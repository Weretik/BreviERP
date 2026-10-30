using System.Text.Json;
using Crm.Api.Quotes.Requests;
namespace IntegrationTests.Crm.Documents;

public sealed class ProductionPackageContractTests
{
    [Test]
    public void Strict_export_request_rejects_browser_costs_and_missing_required_fields()
    {
        var id = Guid.NewGuid();
        var valid = $"{{\"requestId\":\"{id}\",\"versionId\":\"{id}\",\"variantIds\":[\"{id}\"]}}";
        Assert.That(JsonSerializer.Deserialize<ExportProductionPackageRequest>(valid, new JsonSerializerOptions(JsonSerializerDefaults.Web)), Is.Not.Null);
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<ExportProductionPackageRequest>(valid[..^1] + ",\"internalCost\":1}", new JsonSerializerOptions(JsonSerializerDefaults.Web)));
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<ExportProductionPackageRequest>("{}", new JsonSerializerOptions(JsonSerializerDefaults.Web)));
    }

    [Test]
    public void Canonical_contract_names_the_three_implemented_operations()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "docs/sdd/contracts/openapi.yaml"))) directory = directory.Parent;
        Assert.That(directory, Is.Not.Null);
        var path = Path.Combine(directory!.FullName, "docs/sdd/contracts/crm/production-packages.openapi.yaml");
        using var contract = JsonDocument.Parse(File.ReadAllText(path));
        var ids = contract.RootElement.GetProperty("paths").EnumerateObject()
            .SelectMany(item => item.Value.EnumerateObject())
            .Where(item => item.Value.ValueKind == JsonValueKind.Object && item.Value.TryGetProperty("operationId", out _))
            .Select(item => item.Value.GetProperty("operationId").GetString()).ToArray();
        Assert.That(ids, Is.EquivalentTo(new[] { "exportCrmProductionPackage", "getCrmProductionPackage", "downloadCrmProductionPackage" }));
    }
}
