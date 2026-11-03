using Media.Application.Abstractions;
using Media.Application.Contracts;

namespace Media.Application.Files.GetFilesByIds;

public sealed class GetMediaFilesByIdsQueryHandler(IMediaDbContext db)
    : IQueryHandler<GetMediaFilesByIdsQuery, Result<IReadOnlyList<MediaFileResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<MediaFileResponse>>> Handle(GetMediaFilesByIdsQuery query, CancellationToken cancellationToken)
    {
        var ids = query.Ids.Distinct().ToList();
        if (ids.Count == 0) return Result.Success<IReadOnlyList<MediaFileResponse>>([]);
        var files = await db.Files.AsNoTracking().Where(x => ids.Contains(x.Id))
            .Select(MediaFileResponse.Projection).ToListAsync(cancellationToken);
        return Result.Success<IReadOnlyList<MediaFileResponse>>(files);
    }
}
