using Catalog.Infrastructure.Pricing.Ppe.SupplierPrices.Records;

namespace Catalog.Infrastructure.Pricing.Ppe.SupplierPrices.Configuration;

public sealed class PpePriceImportRunConfiguration : IEntityTypeConfiguration<PpePriceImportRunRecord>
{
    public void Configure(EntityTypeBuilder<PpePriceImportRunRecord> builder)
    {
        builder.ToTable("PpePriceImportRuns", "catalog");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();
        builder.Property(x => x.SourceKey).HasMaxLength(40).IsRequired();
        builder.Property(x => x.Trigger).HasMaxLength(40).IsRequired();
        builder.Property(x => x.Actor).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ProblemsJson).HasColumnType("jsonb").IsRequired();
        builder.HasIndex(x => new { x.SourceKey, x.Id });
    }
}
