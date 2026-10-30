using System.Text.Json.Serialization;
namespace Crm.Api.Quotes.Requests;
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record ExportProductionPackageRequest(
    [property: JsonRequired] Guid RequestId,
    [property: JsonRequired] Guid VersionId,
    [property: JsonRequired] IReadOnlyList<Guid> VariantIds);
