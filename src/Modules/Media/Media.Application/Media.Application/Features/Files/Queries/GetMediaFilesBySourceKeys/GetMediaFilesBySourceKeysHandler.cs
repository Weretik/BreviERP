using Media.Application.Abstractions;

namespace Media.Application.Features.Files.Queries.GetMediaFilesBySourceKeys;

public sealed class GetMediaFilesBySourceKeysHandler(IMediaDbContext db)
    : IQueryHandler<GetMediaFilesBySourceKeysQuery, Result<IReadOnlyDictionary<string, int>>>
{
    public async ValueTask<Result<IReadOnlyDictionary<string, int>>> Handle(
        GetMediaFilesBySourceKeysQuery query, CancellationToken cancellationToken)
    {
        var keys = query.SourceKeys.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct().ToList();
        if (keys.Count == 0) return Result.Success<IReadOnlyDictionary<string, int>>(new Dictionary<string, int>());
        var rows = await db.Files.AsNoTracking().Where(x => x.SourceKey != null && keys.Contains(x.SourceKey))
            .Select(x => new { Key = x.SourceKey!, x.Id }).ToListAsync(cancellationToken);
        return Result.Success<IReadOnlyDictionary<string, int>>(rows.ToDictionary(x => x.Key, x => x.Id));
    }
}
