namespace Accounting.Domain.Audit;

/// <summary>Identity of <see cref="AuditEntry"/>.</summary>
public readonly record struct AuditEntryId(long Value);
