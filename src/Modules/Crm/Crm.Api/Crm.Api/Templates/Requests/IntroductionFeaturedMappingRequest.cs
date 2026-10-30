using System.Text.Json.Serialization;

namespace Crm.Api.Templates.Requests;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record IntroductionFeaturedMappingRequest(
    [property: JsonRequired] string SnapshotHash,
    [property: JsonRequired] string SummerModelCode,
    [property: JsonRequired] string WinterModelCode);
