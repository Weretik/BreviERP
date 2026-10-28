using System.Text.Json.Serialization;

namespace Crm.Api.Files.Requests;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CustomFileSelectionRequest([property: JsonRequired] Guid FileId,
    [property: JsonRequired] bool ShowInQuote, [property: JsonRequired] bool Production);
