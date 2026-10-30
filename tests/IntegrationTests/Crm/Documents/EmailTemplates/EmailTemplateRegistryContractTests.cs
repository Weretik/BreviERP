using System.Reflection;
using System.Text.Json;
using Crm.Api.Documents.EmailTemplates;
using Microsoft.AspNetCore.Authorization;

namespace IntegrationTests.Crm.Documents.EmailTemplates;
public sealed class EmailTemplateRegistryContractTests
{
    [Test]
    public void Requests_reject_unknown_properties_and_manager_cannot_write()
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<CreateEmailTemplateRequest>(
            "{\"requestId\":\"00000000-0000-0000-0000-000000000001\",\"name\":\"A\",\"subject\":\"B\",\"body\":\"C\",\"secret\":\"no\"}", new JsonSerializerOptions(JsonSerializerDefaults.Web)));
        foreach (var name in new[] { "Create", "Activate" })
            Assert.That(typeof(EmailTemplatesController).GetMethod(name)!.GetCustomAttribute<AuthorizeAttribute>()!.Roles, Is.EqualTo("Admin"));
    }
    [Test]
    public void Canonical_contract_has_five_operations_strict_requests_and_paged_envelope()
    {
        var dir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "docs/sdd/contracts/crm/email-templates.openapi.yaml"))) dir = dir.Parent;
        using var json = JsonDocument.Parse(File.ReadAllText(Path.Combine(dir!.FullName, "docs/sdd/contracts/crm/email-templates.openapi.yaml")));
        var paths = json.RootElement.GetProperty("paths");
        var ids = paths.EnumerateObject().SelectMany(p => p.Value.EnumerateObject()).Where(x => x.Name != "parameters")
            .Select(x => x.Value.GetProperty("operationId").GetString()).ToArray();
        Assert.That(ids, Is.EquivalentTo(new[] { "getCrmEmailTemplates", "createCrmEmailTemplateVersion", "getCrmEmailTemplateVersion", "getCrmActiveEmailTemplate", "activateCrmEmailTemplate" }));
        var schemas = json.RootElement.GetProperty("components").GetProperty("schemas");
        Assert.That(schemas.GetProperty("CrmCreateEmailTemplateRequest").GetProperty("additionalProperties").GetBoolean(), Is.False);
        Assert.That(schemas.GetProperty("CrmEmailTemplatePage").GetProperty("properties").TryGetProperty("pagedInfo", out _), Is.True);
    }
}
