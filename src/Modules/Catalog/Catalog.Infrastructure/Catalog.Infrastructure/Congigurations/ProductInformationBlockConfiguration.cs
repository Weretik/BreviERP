using Catalog.Domain.Products.Entities;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Infrastructure.Congigurations;

public sealed class ProductInformationBlockConfiguration : IEntityTypeConfiguration<ProductInformationBlock>
{
    public void Configure(EntityTypeBuilder<ProductInformationBlock> builder)
    {
        builder.ToTable("ProductInformationBlocks", "catalog");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ProductId).HasConversion(x => x.Value, x => ProductId.Create(x));
        builder.Property(x => x.TitleUk).HasMaxLength(200).IsRequired();
        builder.Property(x => x.TitleRu).HasMaxLength(200).IsRequired();
        builder.Property(x => x.TextUk).HasMaxLength(2_000).IsRequired();
        builder.Property(x => x.TextRu).HasMaxLength(2_000).IsRequired();
        builder.Property(x => x.SortOrder).IsRequired();
    }
}
