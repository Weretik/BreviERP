using Catalog.Domain.ProductCategories.Entities;
using Catalog.Domain.ProductCategories.ValueObjects;
using Catalog.Domain.Products.Entities;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Infrastructure.Congigurations;

public sealed class ProductCategoryReferenceConfiguration : IEntityTypeConfiguration<ProductCategoryReference>
{
    public void Configure(EntityTypeBuilder<ProductCategoryReference> builder)
    {
        builder.ToTable("ProductCategoryLinks", "catalog");
        builder.Property(x => x.ProductId).HasConversion(x => x.Value, x => ProductId.Create(x));
        builder.Property(x => x.Id).HasColumnName("CategoryId").HasConversion(x => x.Value, x => ProductCategoryId.Create(x)).ValueGeneratedNever();
        builder.HasKey(x => new { x.ProductId, x.Id });
        builder.Ignore(x => x.CategoryId);
        builder.HasOne<ProductCategory>().WithMany().HasForeignKey(x => x.Id).OnDelete(DeleteBehavior.Restrict);
    }
}
