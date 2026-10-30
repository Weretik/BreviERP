using System.Text.Json;
using Crm.Api.Documents.Requests;

namespace IntegrationTests.Crm.Documents;

public sealed class IssuerSettingsRequestContractTests
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    [Test]
    public void Create_and_activate_requests_reject_unknown_fields()
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<CreateIssuerSettingsVersionRequest>(
            """{"requestId":"fa3639f3-2ac4-4618-907f-4f0a95c8e120","unexpected":"internal"}""", Options));
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<ActivateIssuerSettingsRequest>(
            """{"requestId":"fa3639f3-2ac4-4618-907f-4f0a95c8e120","unexpected":"internal"}""", Options));
    }
}
