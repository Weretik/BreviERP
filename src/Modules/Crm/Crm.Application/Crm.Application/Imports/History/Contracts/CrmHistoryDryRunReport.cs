namespace Crm.Application.Imports.History.Contracts;

public sealed record CrmHistoryDryRunReport(
    string Mode,
    string DatasetId,
    string DatabaseSha256,
    string ManifestSha256,
    string Status,
    int DatabaseWrites,
    bool ImportAuthorized,
    CrmHistoryDryRunCounts Counts,
    IReadOnlyList<string> Blockers,
    IReadOnlyList<CrmHistoryTable> Tables,
    IReadOnlyList<CrmHistoryRowDecision> Rows,
    IReadOnlyList<CrmHistoryLedgerEntry> StagingLedger)
{
    public string? ControlSha256 { get; init; }
    public string? OptionsSha256 { get; init; }
}
