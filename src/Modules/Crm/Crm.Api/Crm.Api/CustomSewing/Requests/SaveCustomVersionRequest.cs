using System.Text.Json.Serialization;

namespace Crm.Api.CustomSewing.Requests;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record SaveCustomVersionRequest([property: JsonRequired] Guid RequestId,
    [property: JsonRequired] long ExpectedRevision, [property: JsonRequired] Guid BasisToken);
