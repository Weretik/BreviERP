using Crm.Domain.CustomSewing.Identifiers;
using Crm.Domain.CustomSewing.Templates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Crm.Infrastructure.Persistence.Configurations.Templates;

public sealed class TemplateVersionConfiguration : IEntityTypeConfiguration<CustomTemplateVersion>
{
    public void Configure(EntityTypeBuilder<CustomTemplateVersion> b)
    {
        b.ToTable("CustomSewingTemplateVersions", t => t.HasCheckConstraint("CK_CustomTemplateVersion_Number", "\"Number\" > 0"));
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(x => x.Value, x => new CustomTemplateVersionId(x)).ValueGeneratedOnAdd().UseIdentityByDefaultColumn();
        b.Property(x => x.TemplateId).HasConversion(x => x.Value, x => new CustomTemplateId(x));
        b.Property(x => x.Introduction).HasMaxLength(4000).IsRequired();
        b.Property(x => x.Name).HasMaxLength(300).IsRequired();
        b.Property(x => x.ProductType).HasMaxLength(100).IsRequired();
        b.Property(x => x.Season).HasMaxLength(20).IsRequired();
        TemplateJsonProperty.Configure(b.Property(x => x.Selections));
        b.HasAlternateKey(x => x.PublicId);
        b.HasIndex(x => new { x.TemplateId, x.Number }).IsUnique();
        b.HasOne(x => x.Template).WithMany().HasForeignKey(x => x.TemplateId).OnDelete(DeleteBehavior.Restrict);
        b.Ignore(x => x.DomainEvents);
    }
}
