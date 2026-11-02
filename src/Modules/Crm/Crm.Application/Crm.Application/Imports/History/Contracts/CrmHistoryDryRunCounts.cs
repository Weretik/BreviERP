namespace Crm.Application.Imports.History.Contracts;

public sealed record CrmHistoryDryRunCounts(
    int SourceRows,
    int WouldImport,
    int AlreadyStaged,
    int AlreadyMapped,
    int NeedsReview,
    int Excluded,
    int Invalid);
