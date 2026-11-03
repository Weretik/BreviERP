using Media.Domain.Files;
using Media.Domain.Folders;

namespace Media.Application.Abstractions;

/// <summary>Persistence port of the Media module; implemented by MediaDbContext.</summary>
public interface IMediaDbContext
{
    DbSet<MediaFolder> Folders { get; }
    DbSet<MediaFile> Files { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
