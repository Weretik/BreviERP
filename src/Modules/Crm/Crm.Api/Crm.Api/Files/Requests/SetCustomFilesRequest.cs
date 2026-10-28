using System.Text.Json.Serialization;

namespace Crm.Api.Files.Requests;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record SetCustomFilesRequest([property: JsonRequired] Guid RequestId,
    [property: JsonRequired] long ExpectedRevision, [property: JsonRequired] IReadOnlyList<CustomFileSelectionRequest> Files);
