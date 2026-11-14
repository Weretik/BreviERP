using Content.Domain.ProductPages;
using Content.Domain.SiteCategories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Content.Infrastructure.Persistence;


namespace Content.Infrastructure.Persistence.Configurations.Pages;

public sealed class SiteCategoryConfiguration : IEntityTypeConfiguration<SiteCategory>
{
    public void Configure(EntityTypeBuilder<SiteCategory> b)
    {
        b.ToTable("SiteCategories");
        b.HasKey(x => x.Id);
        b.Property(x => x.Slug).HasMaxLength(ProductPageRules.SlugMaxLength);
        b.HasIndex(x => x.Slug).IsUnique();
        b.Property(x => x.Revision).IsConcurrencyToken();
        b.Property(x => x.UpdatedBy).HasMaxLength(ProductPageRules.UpdatedByMaxLength);
        b.HasIndex(x => new { x.ParentId, x.SortOrder });
        // A category with subcategories is never deleted (CNT-003 R-04).
        b.HasOne<SiteCategory>().WithMany().HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Restrict);
        b.HasMany(x => x.Texts).WithOne().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Texts).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.HasMany<SiteCategoryProduct>().WithOne().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class SiteCategoryTextConfiguration : IEntityTypeConfiguration<SiteCategoryText>
{
    public void Configure(EntityTypeBuilder<SiteCategoryText> b)
    {
        b.ToTable("SiteCategoryTexts");
        b.HasKey(x => new { x.CategoryId, x.Language });
        b.Property(x => x.Language).HasMaxLength(2);
        b.Property(x => x.Name).HasMaxLength(SiteCategoryRules.NameMaxLength);
        b.Property(x => x.SeoTitle).HasMaxLength(ProductPageRules.SeoTitleMaxLength);
        b.Property(x => x.SeoDescription).HasMaxLength(ProductPageRules.SeoDescriptionMaxLength);
        b.Property(x => x.Text).IsJsonList();
    }
}

public sealed class SiteCategoryProductConfiguration : IEntityTypeConfiguration<SiteCategoryProduct>
{
    public void Configure(EntityTypeBuilder<SiteCategoryProduct> b)
    {
        b.ToTable("SiteCategoryProducts");
        b.HasKey(x => new { x.CategoryId, x.ProductId });
        // The Catalog product id; no FK to another module.
        b.HasIndex(x => x.ProductId);
    }
}
