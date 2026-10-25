using Catalog.Domain.Pricing.Sewing;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Catalog.Infrastructure.Pricing.Configuration;

public sealed class SewingPricingSettingsVersionConfiguration : IEntityTypeConfiguration<SewingPricingSettingsVersion>
{
    public void Configure(EntityTypeBuilder<SewingPricingSettingsVersion> builder)
    {
        builder.ToTable("SewingPricingSettingsVersions", "catalog");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, x => SewingPricingSettingsVersionId.Create(x))
            .ValueGeneratedOnAdd().HasValueGenerator<SewingPricingSettingsVersionIdGenerator>();
        builder.HasIndex(x => x.Revision).IsUnique();
        builder.HasIndex(x => x.SourceHash).IsUnique();
        builder.HasIndex(x => x.EffectiveFrom);
        builder.Property(x => x.SourceHash).HasMaxLength(64).IsRequired();
        builder.Property(x => x.SourceJson).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.FormulaVersion).HasMaxLength(80).IsRequired();
        foreach (var property in builder.Metadata.GetProperties().Where(x => x.ClrType == typeof(decimal)))
        {
            property.SetPrecision(20);
            property.SetScale(8);
        }
    }
}

