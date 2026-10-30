using System.Text.Json.Serialization;

namespace Crm.Api.Delivery;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EnqueueQuoteDeliveryRequest
{
    [JsonRequired] public Guid RequestId { get; init; }
    [JsonRequired] public long ExpectedRevision { get; init; }
    [JsonRequired] public Guid VersionId { get; init; }
    [JsonRequired] public Guid ArtifactId { get; init; }
    [JsonRequired] public Guid EmailTemplateVersionId { get; init; }
    [JsonRequired] public string RecipientAddress { get; init; } = null!;
    [JsonRequired] public string ManagerName { get; init; } = null!;
}
