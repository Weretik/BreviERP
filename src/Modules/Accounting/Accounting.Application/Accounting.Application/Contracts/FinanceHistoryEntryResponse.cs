using Accounting.Domain.Audit;

namespace Accounting.Application.Contracts;

/// <summary>One change of a record: snapshots are the JSON text of the record before and after (null when absent).</summary>
public sealed record FinanceHistoryEntryResponse(
    long Id,
    AuditEntity Entity,
    Guid EntityId,
    AuditAction Action,
    string By,
    DateTimeOffset At,
    string? BeforeJson,
    string? AfterJson);
