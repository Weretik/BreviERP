using Media.Domain.Files;
using Media.Domain.Folders;

namespace Media.Infrastructure.Persistence.Configurations;

internal sealed class MediaFileConfiguration : IEntityTypeConfiguration<MediaFile>
{
    public void Configure(EntityTypeBuilder<MediaFile> builder)
    {
        builder.ToTable("Files");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();
        builder.Property(x => x.FileName).HasMaxLength(MediaFile.FileNameMaxLength).IsRequired();
        builder.Property(x => x.Alt).HasMaxLength(MediaFile.AltMaxLength);
        builder.Property(x => x.ContentType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.StorageProvider).HasMaxLength(100).IsRequired();
        builder.Property(x => x.StorageKey).HasMaxLength(500).IsRequired();
        builder.Property(x => x.PublicUrl).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.ThumbnailStorageKey).HasMaxLength(500);
        builder.Property(x => x.ThumbnailUrl).HasMaxLength(1000);
        builder.HasOne<MediaFolder>().WithMany().HasForeignKey(x => x.FolderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.StorageKey).IsUnique();
        builder.HasIndex(x => x.FolderId);
        builder.HasIndex(x => x.ImageProcessedAt).HasFilter("\"ImageProcessedAt\" IS NULL");
    }
}
