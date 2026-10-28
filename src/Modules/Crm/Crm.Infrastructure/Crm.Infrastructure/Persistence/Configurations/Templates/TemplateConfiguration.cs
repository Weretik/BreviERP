using Crm.Domain.Customers.Identifiers;
using Crm.Domain.CustomSewing.Identifiers;
using Crm.Domain.CustomSewing.Templates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Crm.Infrastructure.Persistence.Configurations.Templates;

public sealed class TemplateConfiguration : IEntityTypeConfiguration<CustomTemplate>
{
    public void Configure(EntityTypeBuilder<CustomTemplate> b)
    {
        b.ToTable("CustomSewingTemplates", t => {
            t.HasCheckConstraint("CK_CustomTemplate_Revision", "\"Revision\" > 0");
            t.HasCheckConstraint("CK_CustomTemplate_Version", "\"VersionNumber\" >= 0");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(x => x.Value, x => new CustomTemplateId(x)).ValueGeneratedOnAdd().UseIdentityByDefaultColumn();
        b.Property(x => x.OwnerId).HasConversion(x => x.Value, x => new CrmUserId(x));
        b.Property(x => x.Revision).IsConcurrencyToken();
        b.HasAlternateKey(x => x.PublicId);
        b.HasIndex(x => new { x.OwnerId, x.Archived, x.UpdatedAt, x.Id });
        b.Ignore(x => x.DomainEvents);
    }
}
