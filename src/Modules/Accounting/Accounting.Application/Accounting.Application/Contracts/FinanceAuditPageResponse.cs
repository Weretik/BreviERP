using Accounting.Domain.Audit;

namespace Accounting.Application.Contracts;

/// <summary>A page of «Журнал змін», newest first.</summary>
public sealed record FinanceAuditPageResponse(IReadOnlyList<FinanceAuditEntryResponse> Items, int Total, int Page,
    int PageSize);

/// <summary>
/// One change of any finance record. <c>Title</c> names the record as it was (name, or date, sum and description);
/// snapshots are the JSON text of the record before and after (null when absent).
/// </summary>
public sealed record FinanceAuditEntryResponse(
    long Id,
    AuditEntity Entity,
    Guid EntityId,
    AuditAction Action,
    string By,
    DateTimeOffset At,
    string Title,
    string? BeforeJson,
    string? AfterJson);
