using Catalog.Domain.Products.Entities;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Infrastructure.Congigurations;

public sealed class ProductCharacteristicTableConfiguration : IEntityTypeConfiguration<ProductCharacteristicTable>
{
    public void Configure(EntityTypeBuilder<ProductCharacteristicTable> builder)
    {
        builder.ToTable("ProductCharacteristicTables", "catalog");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ProductId).HasConversion(x => x.Value, x => ProductId.Create(x));
        builder.Property(x => x.TitleUk).HasMaxLength(200).IsRequired();
        builder.Property(x => x.TitleRu).HasMaxLength(200).IsRequired();
        builder.Property(x => x.SortOrder).IsRequired();
        builder.HasMany(x => x.Rows).WithOne().HasForeignKey(x => x.TableId).OnDelete(DeleteBehavior.Cascade);
    }
}
