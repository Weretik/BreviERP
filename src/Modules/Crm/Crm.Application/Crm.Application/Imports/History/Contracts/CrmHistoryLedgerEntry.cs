namespace Crm.Application.Imports.History.Contracts;

public sealed record CrmHistoryLedgerEntry(string Entity, string SourceId, string PayloadSha256);
