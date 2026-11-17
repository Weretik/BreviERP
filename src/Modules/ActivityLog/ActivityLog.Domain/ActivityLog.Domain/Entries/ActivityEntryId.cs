namespace ActivityLog.Domain.Entries;

/// <summary>Identity of <see cref="ActivityEntry"/>.</summary>
public readonly record struct ActivityEntryId(Guid Value);
