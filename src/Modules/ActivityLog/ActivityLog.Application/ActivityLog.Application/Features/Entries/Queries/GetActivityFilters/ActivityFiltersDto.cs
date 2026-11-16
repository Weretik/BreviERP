namespace ActivityLog.Application.Features.Entries.Queries.GetActivityFilters;

/// <summary>What the filters of «Журнал дій» offer: people and object types seen in the journal, every action.</summary>
public sealed record ActivityFiltersDto(
    IReadOnlyList<ActivityActorDto> Actors,
    IReadOnlyList<ActivitySubjectTypeDto> SubjectTypes,
    IReadOnlyList<string> Actions);

public sealed record ActivityActorDto(int Id, string Name);

public sealed record ActivitySubjectTypeDto(string Key, string Title);
