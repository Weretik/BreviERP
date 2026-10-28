using Crm.Domain.CustomSewing.Attachments;
using Crm.Domain.CustomSewing.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations.Files;

public sealed class CustomVersionAttachmentConfiguration : IEntityTypeConfiguration<CustomVersionAttachment>
{
    public void Configure(EntityTypeBuilder<CustomVersionAttachment> b)
    {
        b.ToTable("CustomVersionAttachments");
        b.HasKey(x => new { x.VersionId, x.FileId });
        b.Property(x => x.DraftId).HasConversion(x => x.Value, x => new CustomDraftId(x));
        b.Property(x => x.VersionId).HasConversion(x => x.Value, x => new CustomVersionId(x));
        b.Property(x => x.FileId).HasConversion(x => x.Value, x => new CustomFileId(x));
        b.Property(x => x.ContentType).HasMaxLength(40).IsRequired();
        b.Property(x => x.Sha256).HasMaxLength(64).IsRequired();
        b.HasOne(x => x.File).WithMany().HasForeignKey(x => new { x.DraftId, x.FileId })
            .HasPrincipalKey(x => new { x.DraftId, x.Id }).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Version).WithMany().HasForeignKey(x => new { x.DraftId, x.VersionId })
            .HasPrincipalKey(x => new { x.DraftId, x.Id }).OnDelete(DeleteBehavior.Restrict);
    }
}
