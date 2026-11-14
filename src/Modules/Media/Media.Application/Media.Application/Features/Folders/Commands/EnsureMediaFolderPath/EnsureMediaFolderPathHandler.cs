using Media.Application.Abstractions;
using Media.Domain;
using Media.Domain.Folders;
using BuildingBlocks.Application;

namespace Media.Application.Features.Folders.Commands.EnsureMediaFolderPath;

public sealed class EnsureMediaFolderPathHandler(IMediaDbContext db, TimeProvider clock)
    : ICommandHandler<EnsureMediaFolderPathCommand, Result<int>>
{
    public async ValueTask<Result<int>> Handle(EnsureMediaFolderPathCommand command, CancellationToken cancellationToken)
    {
        int? parentId = null;
        foreach (var raw in command.Names)
        {
            var normalized = MediaNames.Normalize(raw, MediaFolder.NameMaxLength);
            if (normalized.IsFailure) return normalized.Error!.ToResult<int>();
            var name = normalized.Value;
            // Siblings are compared in memory: folders are few, and Cyrillic case must match on every database.
            var siblings = await db.Folders.Where(x => x.ParentId == parentId).ToListAsync(cancellationToken);
            var existing = siblings.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
            if (existing is null)
            {
                existing = MediaFolder.Create(name, parentId, clock.GetUtcNow()).Value;
                db.Folders.Add(existing);
                await db.SaveChangesAsync(cancellationToken);
            }

            parentId = existing.Id;
        }

        return Result.Success(parentId!.Value);
    }
}
