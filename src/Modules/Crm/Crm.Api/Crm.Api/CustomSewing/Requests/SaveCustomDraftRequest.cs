using System.Text.Json.Serialization;

namespace Crm.Api.CustomSewing.Requests;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record SaveCustomDraftRequest([property: JsonRequired] Guid RequestId,
    [property: JsonRequired] long ExpectedRevision, [property: JsonRequired] CustomConfigurationRequest Configuration);
