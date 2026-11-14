using Catalog.Domain.Products;
using Catalog.Domain.Products.Sewing.Composition;
using Catalog.Domain.Products.ValueObjects;


namespace Catalog.Infrastructure.Persistence.Configurations.Sewing.Composition;

public sealed class ProductFabricConfiguration : IEntityTypeConfiguration<ProductFabric>
{
    public void Configure(EntityTypeBuilder<ProductFabric> builder)
    {
        builder.ToTable("SewingProductFabrics", "catalog");
        builder.Property(x => x.ProductId).HasConversion(x => x.Value, x => ProductId.Create(x));
        builder.HasKey(x => new { x.ProductId, x.FabricId });
        builder.Property(x => x.FabricId).IsRequired();
        builder.Property(x => x.IsPrimary).IsRequired();
        builder.Property(x => x.SortOrder).IsRequired();
    }
}
