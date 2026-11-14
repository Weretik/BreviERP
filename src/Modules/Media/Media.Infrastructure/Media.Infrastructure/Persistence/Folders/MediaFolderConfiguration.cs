using Media.Domain.Folders;

namespace Media.Infrastructure.Persistence.Folders;

internal sealed class MediaFolderConfiguration : IEntityTypeConfiguration<MediaFolder>
{
    public void Configure(EntityTypeBuilder<MediaFolder> builder)
    {
        builder.ToTable("Folders");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();
        builder.Property(x => x.Name).HasMaxLength(MediaFolder.NameMaxLength).IsRequired();
        builder.HasOne<MediaFolder>().WithMany().HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.ParentId);
    }
}
