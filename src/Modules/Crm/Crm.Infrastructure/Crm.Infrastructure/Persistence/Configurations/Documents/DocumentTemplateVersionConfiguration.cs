using Crm.Domain.Customers.Identifiers;
using Crm.Domain.Documents.Templates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations.Documents;

public sealed class DocumentTemplateVersionConfiguration : IEntityTypeConfiguration<DocumentTemplateVersion>
{
    public void Configure(EntityTypeBuilder<DocumentTemplateVersion> b)
    {
        b.ToTable("DocumentTemplateVersions", t => t.HasCheckConstraint("CK_DocumentTemplateVersion_Number", "\"Number\" > 0"));
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(x => x.Value, x => new DocumentTemplateVersionId(x))
            .ValueGeneratedOnAdd().UseIdentityByDefaultColumn();
        b.Property(x => x.VerifiedBy).HasConversion(x => x.Value, x => new CrmUserId(x));
        b.Property(x => x.LayoutKey).HasMaxLength(40).IsRequired();
        b.Property(x => x.Title).HasMaxLength(200).IsRequired();
        b.Property(x => x.VatNotice).HasMaxLength(300).IsRequired();
        b.Property(x => x.OpeningText).HasMaxLength(3000).IsRequired();
        b.Property(x => x.CtaTitle).HasMaxLength(500).IsRequired();
        b.Property(x => x.CtaText).HasMaxLength(3000).IsRequired();
        b.Property(x => x.CtaContactTemplate).HasMaxLength(2000).IsRequired();
        b.HasIndex(x => x.PublicId).IsUnique();
        b.HasIndex(x => x.Number).IsUnique();
        b.Ignore(x => x.DomainEvents);
    }
}
