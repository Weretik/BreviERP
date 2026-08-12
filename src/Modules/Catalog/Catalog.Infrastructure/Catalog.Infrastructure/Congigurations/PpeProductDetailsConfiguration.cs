using Catalog.Domain.Products.Entities;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Infrastructure.Congigurations;

public sealed class PpeProductDetailsConfiguration : IEntityTypeConfiguration<PpeProductDetails>
{
    public void Configure(EntityTypeBuilder<PpeProductDetails> builder)
    {
        builder.ToTable("PpeProductDetails", "catalog");
        builder.Property(x => x.ProductId).HasConversion(x => x.Value, x => ProductId.Create(x));
        builder.HasKey(x => x.ProductId);
        builder.Property(x => x.SupplierId).IsRequired();
        builder.Property(x => x.BasePrice).IsRequired();
        builder.ComplexProperty(x => x.RetailPercent, percent =>
        {
            percent.Property(x => x.Source).HasColumnName("RetailPercentSource").HasConversion<string>().IsRequired();
            percent.Property(x => x.AdditionalReferenceId).HasColumnName("RetailAdditionalReferenceId");
            percent.Property(x => x.CustomPercent).HasColumnName("RetailCustomPercent");
        });
        builder.ComplexProperty(x => x.WholesalePercent, percent =>
        {
            percent.Property(x => x.Source).HasColumnName("WholesalePercentSource").HasConversion<string>().IsRequired();
            percent.Property(x => x.AdditionalReferenceId).HasColumnName("WholesaleAdditionalReferenceId");
            percent.Property(x => x.CustomPercent).HasColumnName("WholesaleCustomPercent");
        });
    }
}
