namespace Crm.Application.Imports.History.Contracts;

public sealed record CrmHistoryDryRunRequest(CrmHistorySnapshot Snapshot, CrmHistoryDryRunOptions Options);
