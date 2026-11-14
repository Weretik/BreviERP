
namespace Catalog.Infrastructure.Persistence.Pricing.PpeSupplierPrices;

public sealed class PpePriceSourceSettingsConfiguration : IEntityTypeConfiguration<PpePriceSourceSettingsRecord>
{
    public void Configure(EntityTypeBuilder<PpePriceSourceSettingsRecord> builder)
    {
        builder.ToTable("PpePriceSources", "catalog");
        builder.HasKey(x => x.Key);
        builder.Property(x => x.Key).HasMaxLength(40);
        builder.Property(x => x.DiscountPercent).HasPrecision(5, 2);
        builder.Property(x => x.UpdatedBy).HasMaxLength(200).IsRequired();
    }
}
