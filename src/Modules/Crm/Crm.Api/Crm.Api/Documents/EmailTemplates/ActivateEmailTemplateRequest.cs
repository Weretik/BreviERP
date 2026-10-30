using System.Text.Json.Serialization;
namespace Crm.Api.Documents.EmailTemplates;
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record ActivateEmailTemplateRequest([property: JsonRequired] Guid RequestId,
    [property: JsonRequired] long ExpectedRevision, [property: JsonRequired] Guid VersionId);
