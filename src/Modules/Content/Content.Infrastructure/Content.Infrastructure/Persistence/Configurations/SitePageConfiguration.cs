using Content.Domain.ProductPages;
using Content.Domain.SitePages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Content.Infrastructure.Persistence.Configurations;

public sealed class SitePageConfiguration : IEntityTypeConfiguration<SitePage>
{
    public void Configure(EntityTypeBuilder<SitePage> b)
    {
        b.ToTable("SitePages");
        b.HasKey(x => x.Key);
        b.Property(x => x.Key).HasMaxLength(SitePage.KeyMaxLength).ValueGeneratedNever();
        b.Property(x => x.TitleUk).HasMaxLength(SitePage.TitleMaxLength);
        b.Property(x => x.TitleRu).HasMaxLength(SitePage.TitleMaxLength);
        b.Property(x => x.DescriptionUk).HasMaxLength(SitePage.DescriptionMaxLength);
        b.Property(x => x.DescriptionRu).HasMaxLength(SitePage.DescriptionMaxLength);
        b.Property(x => x.TextUk).IsJsonList();
        b.Property(x => x.TextRu).IsJsonList();
        b.Property(x => x.FaqUk).IsJsonList();
        b.Property(x => x.FaqRu).IsJsonList();
        b.Property(x => x.Revision).IsConcurrencyToken();
        b.Property(x => x.UpdatedBy).HasMaxLength(ProductPageRules.UpdatedByMaxLength);
    }
}
