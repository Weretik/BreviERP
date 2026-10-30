using System.Text.Json.Serialization;

namespace Crm.Api.Templates.Requests;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record IntroductionTemplateArchiveRequest(
    [property: JsonRequired] Guid RequestId,
    [property: JsonRequired] long ExpectedRevision,
    [property: JsonRequired] bool Archived);
