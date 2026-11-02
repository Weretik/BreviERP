namespace Crm.Application.Imports.History.Contracts;

public sealed record CrmHistoryRow(
    string Entity,
    string SourceId,
    string PayloadSha256,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> OwnerKeys,
    IReadOnlyList<CrmHistoryReference> References,
    IReadOnlyList<string> ExternalIdentityKeys,
    string? Kind,
    IReadOnlyList<string> Issues);
