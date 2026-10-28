using Crm.Domain.CustomSewing.Identifiers;
using Crm.Domain.CustomSewing.Versions;
using Crm.Domain.Customers.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations.CustomSewing;

public sealed class CustomVersionConfiguration : IEntityTypeConfiguration<CustomSewingVersion>
{
    public void Configure(EntityTypeBuilder<CustomSewingVersion> builder)
    {
        builder.ToTable("CustomSewingVersions", table => table.HasCheckConstraint("CK_CustomVersion_Number", "\"Number\" > 0"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, x => new CustomVersionId(x)).ValueGeneratedOnAdd().UseIdentityByDefaultColumn();
        builder.Property(x => x.DraftId).HasConversion(x => x.Value, x => new CustomDraftId(x));
        builder.Property(x => x.ParentVersionId).HasConversion(x => x.HasValue ? x.Value.Value : (long?)null,
            x => x.HasValue ? new CustomVersionId(x.Value) : null);
        builder.Property(x => x.ActorId).HasConversion(x => x.Value, x => new CrmUserId(x));
        builder.Property(x => x.Name).HasMaxLength(300).IsRequired();
        CustomJsonProperty.Configure(builder.Property(x => x.Configuration));
        CustomJsonProperty.Configure(builder.Property(x => x.Basis));
        builder.HasOne(x => x.Draft).WithMany().HasForeignKey(x => x.DraftId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ParentVersion).WithMany().HasForeignKey(x => new { x.DraftId, x.ParentVersionId })
            .HasPrincipalKey(x => new { x.DraftId, x.Id }).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.PublicId).IsUnique();
        builder.HasIndex(x => new { x.DraftId, x.Number }).IsUnique();
        builder.Ignore(x => x.DomainEvents);
    }
}
