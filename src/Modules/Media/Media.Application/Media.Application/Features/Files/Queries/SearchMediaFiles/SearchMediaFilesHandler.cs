using Media.Application.Abstractions;
using Media.Application.Features.Files.Shared;
using Media.Application.Features.Folders.Shared;

namespace Media.Application.Features.Files.Queries.SearchMediaFiles;

public sealed class SearchMediaFilesHandler(IMediaDbContext db)
    : IQueryHandler<SearchMediaFilesQuery, Result<MediaFilePage>>
{
    public async ValueTask<Result<MediaFilePage>> Handle(SearchMediaFilesQuery query, CancellationToken cancellationToken)
    {
        var files = db.Files.AsNoTracking();
        if (query.FolderId is { } folderId)
        {
            if (!query.IncludeSubfolders)
            {
                files = files.Where(x => x.FolderId == folderId);
            }
            else
            {
                var tree = await FolderLookup.LoadTreeAsync(db, cancellationToken);
                if (!tree.Contains(folderId)) return Result<MediaFilePage>.NotFound(FileMessages.FolderNotFound);
                var ids = tree.SelfAndDescendants(folderId).ToList();
                files = files.Where(x => x.FolderId != null && ids.Contains(x.FolderId.Value));
            }
        }

        var term = query.Search?.Trim().ToLower();
        if (!string.IsNullOrEmpty(term)) files = files.Where(x => x.FileName.ToLower().Contains(term));

        var total = await files.CountAsync(cancellationToken);
        var items = await files
            .OrderByDescending(x => x.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(MediaFileResponse.Projection)
            .ToListAsync(cancellationToken);
        return Result.Success(new MediaFilePage(items, total, query.Page, query.PageSize));
    }
}
