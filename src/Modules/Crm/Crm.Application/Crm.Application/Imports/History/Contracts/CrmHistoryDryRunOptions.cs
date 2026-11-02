namespace Crm.Application.Imports.History.Contracts;

public sealed record CrmHistoryDryRunOptions(
    IReadOnlyDictionary<string, int> OwnerMappings,
    IReadOnlyDictionary<string, string> ExactIdentityMappings,
    IReadOnlyList<CrmHistoryLedgerEntry> PreviousStaging,
    string? ScopeApprovalReference,
    string? RetentionPolicyReference,
    string? StagingDatasetId = null);
