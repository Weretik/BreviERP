using System.Text.Json.Serialization;

namespace Crm.Infrastructure.Imports.History.Source;

internal sealed class CrmHistoryFrozenSource
{
    [JsonRequired] public int SchemaVersion { get; init; }
    [JsonRequired] public string DatasetId { get; init; } = "";
    [JsonRequired] public string DatabaseFile { get; init; } = "";
    [JsonRequired] public string DatabaseSha256 { get; init; } = "";
    [JsonRequired] public string ManifestFile { get; init; } = "";
    [JsonRequired] public string ManifestSha256 { get; init; } = "";
    public string? OptionsFile { get; init; }
    public string? OptionsSha256 { get; init; }
    public IReadOnlyList<CrmHistoryFrozenBlob> Blobs { get; init; } = [];
}
