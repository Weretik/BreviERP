using Crm.Domain.Customers.Identifiers;
using Crm.Domain.Quotes.Introductions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations.Introductions;

public sealed class IntroductionFeaturedMappingVersionConfiguration : IEntityTypeConfiguration<IntroductionFeaturedMappingVersion>
{
    public void Configure(EntityTypeBuilder<IntroductionFeaturedMappingVersion> builder)
    {
        builder.ToTable("IntroductionFeaturedMappingVersions", table =>
        {
            table.HasCheckConstraint("CK_IntroductionFeaturedMappingVersion_Products",
                "\"SummerProductId\" > 0 AND \"WinterProductId\" > 0 AND \"SummerProductId\" <> \"WinterProductId\"");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd().UseIdentityByDefaultColumn();
        builder.HasAlternateKey(x => x.PublicId);
        builder.Property(x => x.SourceSnapshotHash).HasMaxLength(64).IsRequired();
        builder.Property(x => x.SummerModelCode).HasMaxLength(80).IsRequired();
        builder.Property(x => x.WinterModelCode).HasMaxLength(80).IsRequired();
        builder.Property(x => x.ApprovedBy).HasConversion(x => x.Value, x => new CrmUserId(x));
        builder.HasIndex(x => x.SourceSnapshotHash);
        builder.Ignore(x => x.DomainEvents);
    }
}
