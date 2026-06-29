using Catalog.Domain.Media.Entities;

namespace Catalog.Infrastructure.Congigurations;

public sealed class MediaFileConfiguration : IEntityTypeConfiguration<MediaFile>
{
    public void Configure(EntityTypeBuilder<MediaFile> builder)
    {
        builder.ToTable("MediaFiles", "catalog");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, x => Catalog.Domain.Media.ValueObjects.MediaFileId.Create(x))
            .ValueGeneratedNever();

        builder.Property(x => x.FileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.ContentType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.SizeInBytes)
            .IsRequired();

        builder.Property(x => x.StorageProvider)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.BucketName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.StorageKey)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.PublicUrl)
            .HasMaxLength(1000);

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.HasIndex(x => x.StorageKey)
            .IsUnique();
    }
}
