namespace ActivityLog.Application.Features.Entries.Queries.GetActivityEntries;

public sealed record ActivityChangeDto(string Field, string? From, string? To);
