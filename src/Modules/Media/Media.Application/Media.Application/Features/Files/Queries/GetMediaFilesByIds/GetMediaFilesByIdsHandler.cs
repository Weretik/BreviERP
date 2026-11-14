using Media.Application.Abstractions;
using Media.Application.Features.Files.Shared;

namespace Media.Application.Features.Files.Queries.GetMediaFilesByIds;

public sealed class GetMediaFilesByIdsHandler(IMediaDbContext db)
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
