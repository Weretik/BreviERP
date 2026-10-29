using Crm.Application.Documents.Contracts;

namespace Crm.Api.Documents.Requests;

[System.Text.Json.Serialization.JsonUnmappedMemberHandling(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)]
public sealed record CreateIssuerSettingsVersionRequest(Guid RequestId, IssuerSettingsInput Issuer,
    DocumentTemplateInput Template);
