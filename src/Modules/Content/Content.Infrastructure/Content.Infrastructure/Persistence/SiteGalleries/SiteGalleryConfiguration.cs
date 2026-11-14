using Content.Domain.ProductPages;
using Content.Domain.SiteGalleries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Content.Infrastructure.Persistence;

namespace Content.Infrastructure.Persistence.SiteGalleries;

/// <summary>One row per gallery (<c>works</c>, <c>certificates</c>); the pictures are one jsonb list read and written as a whole.</summary>
public sealed class SiteGalleryConfiguration : IEntityTypeConfiguration<SiteGallery>
{
    public void Configure(EntityTypeBuilder<SiteGallery> b)
    {
        b.ToTable("SiteGalleries");
        b.HasKey(x => x.Key);
        b.Property(x => x.Key).HasMaxLength(SiteGallery.KeyMaxLength).ValueGeneratedNever();
        b.Property(x => x.Items).IsJsonList();
        b.Property(x => x.Revision).IsConcurrencyToken();
        b.Property(x => x.UpdatedBy).HasMaxLength(ProductPageRules.UpdatedByMaxLength);
    }
}
