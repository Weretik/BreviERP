namespace Crm.Api.Documents.Requests;

[System.Text.Json.Serialization.JsonUnmappedMemberHandling(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)]
public sealed record ActivateIssuerSettingsRequest(Guid RequestId, long ExpectedRevision,
    Guid IssuerVersionId, Guid TemplateVersionId);
