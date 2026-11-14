using Content.Domain.ProductPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Content.Infrastructure.Persistence;


namespace Content.Infrastructure.Persistence.Configurations.Pages;

public sealed class ProductPageConfiguration : IEntityTypeConfiguration<ProductPage>
{
    public void Configure(EntityTypeBuilder<ProductPage> b)
    {
        b.ToTable("ProductPages");
        b.HasKey(x => x.ProductId);
        // The id is the Catalog product id, never generated here.
        b.Property(x => x.ProductId).ValueGeneratedNever();
        b.Property(x => x.Slug).HasMaxLength(ProductPageRules.SlugMaxLength);
        b.HasIndex(x => x.Slug).IsUnique();
        b.Property(x => x.Revision).IsConcurrencyToken();
        b.Property(x => x.UpdatedBy).HasMaxLength(ProductPageRules.UpdatedByMaxLength);
        b.HasIndex(x => x.ShowOnSite);
        b.HasMany(x => x.Texts).WithOne().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Texts).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class ProductPageTextConfiguration : IEntityTypeConfiguration<ProductPageText>
{
    public void Configure(EntityTypeBuilder<ProductPageText> b)
    {
        b.ToTable("ProductPageTexts");
        b.HasKey(x => new { x.ProductId, x.Language });
        b.Property(x => x.Language).HasMaxLength(2);
        b.Property(x => x.Name).HasMaxLength(ProductPageRules.NameMaxLength);
        b.Property(x => x.SeoTitle).HasMaxLength(ProductPageRules.SeoTitleMaxLength);
        b.Property(x => x.SeoDescription).HasMaxLength(ProductPageRules.SeoDescriptionMaxLength);
        b.Property(x => x.Summary).IsJsonList();
        b.Property(x => x.Description).IsJsonList();
        b.Property(x => x.Specs).IsJsonList();
    }
}
