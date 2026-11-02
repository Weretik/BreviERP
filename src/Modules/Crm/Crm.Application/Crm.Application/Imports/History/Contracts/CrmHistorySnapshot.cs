namespace Crm.Application.Imports.History.Contracts;

public sealed record CrmHistorySnapshot(
    string DatasetId,
    string DatabaseSha256,
    string ManifestSha256,
    bool FullBackup,
    bool TransactionConsistent,
    IReadOnlyList<CrmHistoryTable> Tables,
    IReadOnlyList<CrmHistoryRow> Rows,
    IReadOnlyList<string> Issues)
{
    public string? ControlSha256 { get; init; }
    public string? OptionsSha256 { get; init; }
}
