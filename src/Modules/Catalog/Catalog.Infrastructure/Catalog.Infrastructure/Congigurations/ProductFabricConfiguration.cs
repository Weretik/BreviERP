using Catalog.Domain.Products.Entities;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Infrastructure.Congigurations;

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
