using Content.Domain.Articles;
using Content.Domain.ProductPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Content.Infrastructure.Persistence.Configurations;

public sealed class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> b)
    {
        b.ToTable("Articles");
        b.HasKey(x => x.Id);
        b.Property(x => x.Slug).HasMaxLength(ProductPageRules.SlugMaxLength);
        b.HasIndex(x => x.Slug).IsUnique();
        b.Property(x => x.Revision).IsConcurrencyToken();
        b.Property(x => x.UpdatedBy).HasMaxLength(ProductPageRules.UpdatedByMaxLength);
        b.HasIndex(x => x.PublishedOn);
        // CoverPhotoId is the id of a Media photo; no FK to another module.
        b.HasMany(x => x.Texts).WithOne().HasForeignKey(x => x.ArticleId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Texts).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class ArticleTextConfiguration : IEntityTypeConfiguration<ArticleText>
{
    public void Configure(EntityTypeBuilder<ArticleText> b)
    {
        b.ToTable("ArticleTexts");
        b.HasKey(x => new { x.ArticleId, x.Language });
        b.Property(x => x.Language).HasMaxLength(2);
        b.Property(x => x.Title).HasMaxLength(ArticleRules.TitleMaxLength);
        b.Property(x => x.Excerpt).HasMaxLength(ArticleRules.ExcerptMaxLength);
        b.Property(x => x.SeoTitle).HasMaxLength(ProductPageRules.SeoTitleMaxLength);
        b.Property(x => x.SeoDescription).HasMaxLength(ProductPageRules.SeoDescriptionMaxLength);
        b.Property(x => x.Body).IsJsonList();
    }
}
