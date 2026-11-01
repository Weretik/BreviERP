using System.Text.Json.Serialization;

namespace Crm.Api.Delivery.Resolution;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class ResolveQuoteDeliveryRequest
{
    [JsonRequired] public Guid RequestId { get; init; }
    [JsonRequired] public int ExpectedAttempt { get; init; }
    [JsonRequired] public string Outcome { get; init; } = null!;
    [JsonRequired] public string EvidenceNote { get; init; } = null!;
}
