using Catalog.Domain.Products.Entities;

namespace Catalog.Infrastructure.Congigurations;

public sealed class ProductCharacteristicRowConfiguration : IEntityTypeConfiguration<ProductCharacteristicRow>
{
    public void Configure(EntityTypeBuilder<ProductCharacteristicRow> builder)
    {
        builder.ToTable("ProductCharacteristicRows", "catalog");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TableId).IsRequired();
        builder.Property(x => x.LabelUk).HasMaxLength(200).IsRequired();
        builder.Property(x => x.LabelRu).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ValueUk).HasMaxLength(1_000).IsRequired();
        builder.Property(x => x.ValueRu).HasMaxLength(1_000).IsRequired();
        builder.Property(x => x.SortOrder).IsRequired();
    }
}
