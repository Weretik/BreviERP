using System.Text.Json.Serialization;

namespace Crm.Api.CustomSewing.Requests;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CreateCustomDraftRequest([property: JsonRequired] Guid RequestId,
    Guid? CustomerPublicId, Guid? QuotePublicId, [property: JsonRequired] CustomConfigurationRequest Configuration);
