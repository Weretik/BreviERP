using System.Text.Json.Serialization;
namespace Crm.Api.Templates.Requests;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record ApplyTemplateRequest([property: JsonRequired] Guid RequestId, [property: JsonRequired] long ExpectedRevision, [property: JsonRequired] Guid VersionId, Guid? CustomerPublicId, Guid? QuotePublicId);
