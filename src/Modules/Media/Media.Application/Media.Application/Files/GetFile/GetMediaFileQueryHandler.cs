using Media.Application.Abstractions;
using Media.Application.Contracts;

namespace Media.Application.Files.GetFile;

public sealed class GetMediaFileQueryHandler(IMediaDbContext db, IMediaUsageReader usages)
    : IQueryHandler<GetMediaFileQuery, Result<MediaFileDetailResponse>>
{
    public async ValueTask<Result<MediaFileDetailResponse>> Handle(GetMediaFileQuery query, CancellationToken cancellationToken)
    {
        var file = await db.Files.AsNoTracking().Where(x => x.Id == query.Id)
            .Select(MediaFileResponse.Projection).FirstOrDefaultAsync(cancellationToken);
        if (file is null) return Result<MediaFileDetailResponse>.NotFound(FileMessages.NotFound);
        var used = await usages.FindAsync([file.Id], cancellationToken);
        return Result.Success(new MediaFileDetailResponse(
            file,
            used.Select(x => new MediaUsageResponse(x.Area, x.OwnerId, x.Title)).ToList()));
    }
}
