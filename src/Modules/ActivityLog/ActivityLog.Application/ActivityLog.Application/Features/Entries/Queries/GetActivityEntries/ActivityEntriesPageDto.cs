namespace ActivityLog.Application.Features.Entries.Queries.GetActivityEntries;

public sealed record ActivityEntriesPageDto(IReadOnlyList<ActivityEntryDto> Items, int Page, int PageSize, int Total);
