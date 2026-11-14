using Catalog.Domain.Products;
using Catalog.Domain.Products.Sewing.Composition;
using Catalog.Domain.Products.ValueObjects;


namespace Catalog.Infrastructure.Persistence.Configurations.Sewing.Composition;

public sealed class ProductOperationConfiguration : IEntityTypeConfiguration<ProductOperation>
{
    public void Configure(EntityTypeBuilder<ProductOperation> builder)
    {
        builder.ToTable("SewingProductOperations", "catalog");
        builder.Property(x => x.ProductId).HasConversion(x => x.Value, x => ProductId.Create(x));
        builder.HasKey(x => new { x.ProductId, x.GarmentPartOperationId });
        builder.Property(x => x.GarmentPartOperationId).IsRequired();
    }
}
