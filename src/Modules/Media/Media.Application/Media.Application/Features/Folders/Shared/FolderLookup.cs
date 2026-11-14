using Media.Application.Abstractions;
using Media.Domain.Folders;

namespace Media.Application.Features.Folders.Shared;

/// <summary>Folder reads shared by the folder and file use cases.</summary>
internal static class FolderLookup
{
    public static async Task<MediaFolderTree> LoadTreeAsync(IMediaDbContext db, CancellationToken cancellationToken)
    {
        var links = await db.Folders.AsNoTracking().Select(x => new { x.Id, x.ParentId }).ToListAsync(cancellationToken);
        return new MediaFolderTree(links.Select(x => (x.Id, x.ParentId)));
    }

    public static Task<bool> NameTakenAsync(IMediaDbContext db, string name, int? parentId, int? exceptId, CancellationToken cancellationToken)
    {
        var lowered = name.ToLower();
        return db.Folders.AnyAsync(
            x => x.ParentId == parentId && x.Name.ToLower() == lowered && (exceptId == null || x.Id != exceptId),
            cancellationToken);
    }
}
