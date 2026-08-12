using Catalog.Domain.Products.Entities;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Infrastructure.Congigurations;

public sealed class SewingProductDetailsConfiguration : IEntityTypeConfiguration<SewingProductDetails>
{
    public void Configure(EntityTypeBuilder<SewingProductDetails> builder)
    {
        builder.ToTable("SewingProductDetails", "catalog");
        builder.Property(x => x.ProductId).HasConversion(x => x.Value, x => ProductId.Create(x));
        builder.HasKey(x => x.ProductId);
        builder.Property(x => x.MetersPerProduct).IsRequired();
        builder.HasMany(x => x.Fabrics).WithOne().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Accessories).WithOne().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Operations).WithOne().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
    }
}
