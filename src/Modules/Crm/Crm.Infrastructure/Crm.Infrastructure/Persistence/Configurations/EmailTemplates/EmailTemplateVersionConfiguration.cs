using Crm.Domain.Customers.Identifiers;
using Crm.Domain.Documents.EmailTemplates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations.EmailTemplates;
public sealed class EmailTemplateVersionConfiguration : IEntityTypeConfiguration<EmailTemplateVersion>
{
    public void Configure(EntityTypeBuilder<EmailTemplateVersion> b)
    {
        b.ToTable("EmailTemplateVersions", t => t.HasCheckConstraint("CK_EmailTemplateVersions_Number", "\"Number\" > 0"));
        b.HasKey(x => x.Id); b.Property(x => x.Id).HasConversion(x => x.Value, x => new EmailTemplateVersionId(x)).ValueGeneratedOnAdd();
        b.Property(x => x.VerifiedBy).HasConversion(x => x.Value, x => new CrmUserId(x));
        b.Property(x => x.Name).HasMaxLength(120).IsRequired(); b.Property(x => x.Subject).HasMaxLength(200).IsRequired();
        b.Property(x => x.Body).HasMaxLength(3000).IsRequired();
        b.HasIndex(x => x.PublicId).IsUnique(); b.HasIndex(x => x.Number).IsUnique();
    }
}
