using System.Text.Json.Serialization;

namespace Crm.Api.Documents.Requests;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record RenderQuotePdfRequest(Guid VersionId);
