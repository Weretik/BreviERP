using Content.Domain.ProductPages;
using Content.Domain.SiteSettings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace Content.Infrastructure.Persistence.Configurations.Site;

public sealed class SiteSeoSettingsConfiguration : IEntityTypeConfiguration<SiteSeoSettings>
{
    public void Configure(EntityTypeBuilder<SiteSeoSettings> b)
    {
        b.ToTable("SiteSeoSettings", t => t.HasCheckConstraint("CK_SiteSeoSettings_Single", "\"Id\" = 1"));
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.GoogleAnalyticsId).HasMaxLength(20);
        b.Property(x => x.GoogleTagManagerId).HasMaxLength(20);
        b.Property(x => x.GoogleSiteVerification).HasMaxLength(SiteSeoSettings.VerificationMaxLength);
        b.Property(x => x.BingSiteVerification).HasMaxLength(SiteSeoSettings.VerificationMaxLength);
        b.Property(x => x.Revision).IsConcurrencyToken();
        b.Property(x => x.UpdatedBy).HasMaxLength(ProductPageRules.UpdatedByMaxLength);
        // DefaultOgPhotoId is the id of a Media photo; no FK to another module.
    }
}
