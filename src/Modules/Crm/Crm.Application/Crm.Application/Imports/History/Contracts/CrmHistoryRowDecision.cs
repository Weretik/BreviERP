namespace Crm.Application.Imports.History.Contracts;

public sealed record CrmHistoryRowDecision(
    string Entity,
    string SourceKeySha256,
    string PayloadSha256,
    string Status,
    IReadOnlyList<string> Reasons);
