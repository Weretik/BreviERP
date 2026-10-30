using System.Text.Json;
using Crm.Api.Delivery;
using Microsoft.AspNetCore.Authorization;

namespace IntegrationTests.Crm.Delivery;

public sealed class QuoteDeliveryOutboxContractTests
{
    [Test]
    public void Queue_request_rejects_missing_fields_and_browser_body_or_pdf()
    {
        var id = Guid.NewGuid();
        var request = JsonSerializer.Serialize(new { requestId = id, expectedRevision = 2, versionId = id,
            artifactId = id, emailTemplateVersionId = id, recipientAddress = "sandbox@example.test", managerName = "Manager" });
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        Assert.That(JsonSerializer.Deserialize<EnqueueQuoteDeliveryRequest>(request, options), Is.Not.Null);
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<EnqueueQuoteDeliveryRequest>("{}", options));
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<EnqueueQuoteDeliveryRequest>(request[..^1] + ",\"subject\":\"forged\"}", options));
        var roles = typeof(QuoteDeliveriesController).GetCustomAttributes(typeof(AuthorizeAttribute), true).Cast<AuthorizeAttribute>().Single().Roles;
        Assert.That(roles, Is.EqualTo("Admin,Manager"));
    }

    [Test]
    public void Canonical_contract_has_three_explicit_delivery_operations_and_safe_detail()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "docs/sdd/contracts/openapi.yaml"))) directory = directory.Parent;
        Assert.That(directory, Is.Not.Null);
        using var json = JsonDocument.Parse(File.ReadAllText(Path.Combine(directory!.FullName, "docs/sdd/contracts/crm/quote-deliveries.openapi.yaml")));
        var operations = json.RootElement.GetProperty("paths").EnumerateObject().SelectMany(x => x.Value.EnumerateObject())
            .Where(x => x.Value.ValueKind == JsonValueKind.Object && x.Value.TryGetProperty("operationId", out _)).Select(x => x.Value.GetProperty("operationId").GetString()).ToArray();
        Assert.That(operations, Is.EquivalentTo(new[] { "enqueueCrmQuoteDelivery", "getCrmQuoteDeliveries", "getCrmQuoteDelivery" }));
        var properties = json.RootElement.GetProperty("components").GetProperty("schemas").GetProperty("QuoteDeliveryDetail").GetProperty("properties");
        Assert.That(properties.TryGetProperty("pdfBytes", out _), Is.False);
        Assert.That(properties.TryGetProperty("plainText", out _), Is.False);
        Assert.That(properties.TryGetProperty("subject", out _), Is.False);
    }
}
