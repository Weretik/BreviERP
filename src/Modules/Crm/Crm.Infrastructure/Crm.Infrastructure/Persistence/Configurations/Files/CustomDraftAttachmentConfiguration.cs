using Crm.Domain.CustomSewing.Attachments;
using Crm.Domain.CustomSewing.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations.Files;

public sealed class CustomDraftAttachmentConfiguration : IEntityTypeConfiguration<CustomDraftAttachment>
{
    public void Configure(EntityTypeBuilder<CustomDraftAttachment> b)
    {
        b.ToTable("CustomDraftAttachments");
        b.HasKey(x => new { x.DraftId, x.FileId });
        b.Property(x => x.DraftId).HasConversion(x => x.Value, x => new CustomDraftId(x));
        b.Property(x => x.FileId).HasConversion(x => x.Value, x => new CustomFileId(x));
        b.HasOne(x => x.File).WithMany().HasForeignKey(x => new { x.DraftId, x.FileId })
            .HasPrincipalKey(x => new { x.DraftId, x.Id }).OnDelete(DeleteBehavior.Restrict);
    }
}
