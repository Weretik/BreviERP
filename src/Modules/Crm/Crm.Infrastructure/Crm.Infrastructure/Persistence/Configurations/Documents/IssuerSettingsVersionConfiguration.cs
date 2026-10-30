using Crm.Domain.Customers.Identifiers;
using Crm.Domain.Documents.Issuer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations.Documents;

public sealed class IssuerSettingsVersionConfiguration : IEntityTypeConfiguration<IssuerSettingsVersion>
{
    public void Configure(EntityTypeBuilder<IssuerSettingsVersion> b)
    {
        b.ToTable("IssuerSettingsVersions", t => t.HasCheckConstraint("CK_IssuerSettingsVersion_Number", "\"Number\" > 0"));
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(x => x.Value, x => new IssuerSettingsVersionId(x))
            .ValueGeneratedOnAdd().UseIdentityByDefaultColumn();
        b.Property(x => x.VerifiedBy).HasConversion(x => x.Value, x => new CrmUserId(x));
        b.Property(x => x.LegalName).HasMaxLength(200).IsRequired();
        b.Property(x => x.Edrpou).HasMaxLength(50).IsRequired();
        b.Property(x => x.TaxId).HasMaxLength(50).IsRequired();
        b.Property(x => x.Iban).HasMaxLength(80).IsRequired();
        b.Property(x => x.Address).HasMaxLength(500).IsRequired();
        b.Property(x => x.Phone).HasMaxLength(80).IsRequired();
        b.Property(x => x.Email).HasMaxLength(200).IsRequired();
        b.Property(x => x.Site).HasMaxLength(500).IsRequired();
        b.Property(x => x.BankName).HasMaxLength(200);
        b.Property(x => x.Mfo).HasMaxLength(20);
        b.Property(x => x.LogoAssetKey).HasMaxLength(200);
        b.HasIndex(x => x.PublicId).IsUnique();
        b.HasIndex(x => x.Number).IsUnique();
        b.Ignore(x => x.DomainEvents);
    }
}
