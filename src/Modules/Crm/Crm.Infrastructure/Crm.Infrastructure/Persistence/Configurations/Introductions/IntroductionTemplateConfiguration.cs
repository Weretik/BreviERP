using Crm.Domain.Customers.Identifiers;
using Crm.Domain.Quotes.Introductions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations.Introductions;

public sealed class IntroductionTemplateConfiguration : IEntityTypeConfiguration<IntroductionTemplate>
{
    public void Configure(EntityTypeBuilder<IntroductionTemplate> builder)
    {
        builder.ToTable("IntroductionTemplates", table =>
        {
            table.HasCheckConstraint("CK_IntroductionTemplate_Revision", "\"Revision\" > 0");
            table.HasCheckConstraint("CK_IntroductionTemplate_Version", "\"VersionNumber\" >= 0");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, x => new IntroductionTemplateId(x))
            .ValueGeneratedOnAdd().UseIdentityByDefaultColumn();
        builder.Property(x => x.OwnerId).HasConversion(x => x.Value, x => new CrmUserId(x));
        builder.Property(x => x.Revision).IsConcurrencyToken();
        builder.HasAlternateKey(x => x.PublicId);
        builder.HasIndex(x => new { x.OwnerId, x.Archived, x.UpdatedAt, x.Id });
        builder.Ignore(x => x.DomainEvents);
    }
}
