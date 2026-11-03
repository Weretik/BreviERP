using Media.Application.Abstractions;
using Media.Domain.Files;
using Media.Domain.Folders;

namespace Media.Infrastructure.Persistence;

public sealed class MediaDbContext(DbContextOptions<MediaDbContext> options) : DbContext(options), IMediaDbContext
{
    public const string Schema = "media";

    public DbSet<MediaFolder> Folders => Set<MediaFolder>();
    public DbSet<MediaFile> Files => Set<MediaFile>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema(Schema);
        builder.ApplyConfigurationsFromAssembly(typeof(MediaDbContext).Assembly);
    }
}
