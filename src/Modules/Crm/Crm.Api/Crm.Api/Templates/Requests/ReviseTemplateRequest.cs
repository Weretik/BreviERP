using System.Text.Json.Serialization;
namespace Crm.Api.Templates.Requests;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record ReviseTemplateRequest([property: JsonRequired] Guid RequestId, [property: JsonRequired] long ExpectedRevision, [property: JsonRequired] string Kind, [property: JsonRequired] string Introduction, [property: JsonRequired] TemplateSelectionsRequest Selections);
