using Media.Application.Abstractions;
using Media.Domain;
using Media.Domain.Folders;

namespace Media.Application.Folders.EnsureFolderPath;

public sealed class EnsureMediaFolderPathCommandHandler(IMediaDbContext db, TimeProvider clock)
    : ICommandHandler<EnsureMediaFolderPathCommand, Result<int>>
{
    public async ValueTask<Result<int>> Handle(EnsureMediaFolderPathCommand command, CancellationToken cancellationToken)
    {
        int? parentId = null;
        foreach (var raw in command.Names)
        {
            var name = MediaNames.Normalize(raw, MediaFolder.NameMaxLength);
            // Siblings are compared in memory: folders are few, and Cyrillic case must match on every database.
            var siblings = await db.Folders.Where(x => x.ParentId == parentId).ToListAsync(cancellationToken);
            var existing = siblings.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
            if (existing is null)
            {
                existing = new MediaFolder(name, parentId, clock.GetUtcNow());
                db.Folders.Add(existing);
                await db.SaveChangesAsync(cancellationToken);
            }

            parentId = existing.Id;
        }

        return Result.Success(parentId!.Value);
    }
}
