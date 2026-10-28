using System.Text.Json.Serialization;

namespace Crm.Api.CustomSewing.Requests;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CalculateCustomSewingRequest([property: JsonRequired] Guid RequestId,
    [property: JsonRequired] long ExpectedRevision);
