namespace Crm.Application.Imports.History.Contracts;

public sealed record CrmHistoryTable(
    string Entity,
    int ExpectedRows,
    int ActualRows,
    bool Complete,
    IReadOnlyList<string> Issues);
