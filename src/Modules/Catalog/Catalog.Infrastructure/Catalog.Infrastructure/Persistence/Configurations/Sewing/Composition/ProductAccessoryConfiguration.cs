using Catalog.Domain.Products;
using Catalog.Domain.Products.Sewing.Composition;
using Catalog.Domain.Products.ValueObjects;


namespace Catalog.Infrastructure.Persistence.Configurations.Sewing.Composition;

public sealed class ProductAccessoryConfiguration : IEntityTypeConfiguration<ProductAccessory>
{
    public void Configure(EntityTypeBuilder<ProductAccessory> builder)
    {
        builder.ToTable("SewingProductAccessories", "catalog");
        builder.Property(x => x.ProductId).HasConversion(x => x.Value, x => ProductId.Create(x));
        builder.HasKey(x => new { x.ProductId, x.GarmentAccessoryId });
        builder.Property(x => x.GarmentAccessoryId).IsRequired();
        builder.Property(x => x.Quantity).IsRequired();
        builder.Property(x => x.SortOrder).IsRequired();
    }
}
