using Media.Application.Abstractions;
using Media.Application.Contracts;

namespace Media.Application.Folders.GetFolders;

public sealed class GetMediaFoldersQueryHandler(IMediaDbContext db)
    : IQueryHandler<GetMediaFoldersQuery, Result<IReadOnlyList<MediaFolderResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<MediaFolderResponse>>> Handle(GetMediaFoldersQuery query, CancellationToken cancellationToken)
    {
        var counts = await db.Files.AsNoTracking()
            .Where(x => x.FolderId != null)
            .GroupBy(x => x.FolderId!.Value)
            .Select(x => new { FolderId = x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.FolderId, x => x.Count, cancellationToken);
        var folders = await db.Folders.AsNoTracking().OrderBy(x => x.Name).ToListAsync(cancellationToken);
        IReadOnlyList<MediaFolderResponse> result = folders
            .Select(x => new MediaFolderResponse(x.Id, x.Name, x.ParentId, counts.GetValueOrDefault(x.Id)))
            .ToList();
        return Result.Success(result);
    }
}
