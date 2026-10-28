using System.Text.Json.Serialization;
namespace Crm.Api.Templates.Requests;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record ArchiveTemplateRequest([property: JsonRequired] Guid RequestId, [property: JsonRequired] long ExpectedRevision, [property: JsonRequired] bool Archived);
