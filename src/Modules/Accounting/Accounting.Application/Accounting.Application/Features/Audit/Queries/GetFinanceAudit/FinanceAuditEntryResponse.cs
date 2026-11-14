using Accounting.Domain.Audit;

namespace Accounting.Application.Features.Audit.Queries.GetFinanceAudit;

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
