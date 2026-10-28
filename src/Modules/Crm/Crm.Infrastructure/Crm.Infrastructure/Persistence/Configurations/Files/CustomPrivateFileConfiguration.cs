using Crm.Domain.Customers.Identifiers;
using Crm.Domain.CustomSewing.Attachments;
using Crm.Domain.CustomSewing.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations.Files;

public sealed class CustomPrivateFileConfiguration : IEntityTypeConfiguration<CustomPrivateFile>
{
    public void Configure(EntityTypeBuilder<CustomPrivateFile> b)
    {
        b.ToTable("CustomPrivateFiles", t =>
        {
            t.HasCheckConstraint("CK_CustomPrivateFile_Length", "\"Length\" > 0 AND \"Length\" <= 20971520");
            t.HasCheckConstraint("CK_CustomPrivateFile_Expiry", "\"ExpiresAt\" > \"CreatedAt\"");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(x => x.Value, x => new CustomFileId(x)).ValueGeneratedOnAdd().UseIdentityByDefaultColumn();
        b.Property(x => x.DraftId).HasConversion(x => x.Value, x => new CustomDraftId(x));
        b.Property(x => x.ActorId).HasConversion(x => x.Value, x => new CrmUserId(x));
        b.Property(x => x.State).HasConversion<string>().HasMaxLength(16).IsConcurrencyToken();
        b.Property(x => x.IntentHash).HasMaxLength(64).IsRequired();
        b.Property(x => x.Sha256).HasMaxLength(64).IsRequired();
        b.Property(x => x.ContentType).HasMaxLength(40).IsRequired();
        b.HasAlternateKey(x => x.PublicId);
        b.HasAlternateKey(x => new { x.DraftId, x.Id });
        b.HasIndex(x => new { x.ActorId, x.RequestId }).IsUnique();
        b.HasIndex(x => new { x.DraftId, x.State });
        b.HasIndex(x => new { x.State, x.ExpiresAt });
        b.HasOne(x => x.Draft).WithMany().HasForeignKey(x => x.DraftId).OnDelete(DeleteBehavior.Restrict);
        b.Ignore(x => x.DomainEvents);
    }
}
