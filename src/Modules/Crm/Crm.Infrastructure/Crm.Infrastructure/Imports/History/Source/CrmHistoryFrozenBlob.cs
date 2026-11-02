using System.Text.Json.Serialization;

namespace Crm.Infrastructure.Imports.History.Source;

internal sealed class CrmHistoryFrozenBlob
{
    [JsonRequired] public string SourceFileId { get; init; } = "";
    [JsonRequired] public string File { get; init; } = "";
    [JsonRequired] public string Sha256 { get; init; } = "";
    [JsonRequired] public long Length { get; init; }
}
