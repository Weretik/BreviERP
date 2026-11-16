namespace ActivityLog.Application.Features.Entries.Queries.GetActivityEntries;

/// <param name="From">First Kyiv day of the period, inclusive.</param>
/// <param name="To">Last Kyiv day of the period, inclusive.</param>
/// <param name="Role">A role name; null for every role.</param>
/// <param name="Search">Words of the object's title or note.</param>
public sealed record GetActivityEntriesQuery(
    int Page = 1,
    int PageSize = GetActivityEntriesQuery.DefaultPageSize,
    int? ActorId = null,
    string? Role = null,
    string? SubjectType = null,
    string? Action = null,
    DateOnly? From = null,
    DateOnly? To = null,
    string? Search = null) : IQuery<Result<ActivityEntriesPageDto>>
{
    public const int DefaultPageSize = 50;
    public const int MaxPageSize = 100;
}
