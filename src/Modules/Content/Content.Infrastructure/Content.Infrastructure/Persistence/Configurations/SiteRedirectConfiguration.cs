using Content.Domain.ProductPages;
using Content.Domain.Redirects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Content.Infrastructure.Persistence.Configurations;

public sealed class SiteRedirectConfiguration : IEntityTypeConfiguration<SiteRedirect>
{
    public void Configure(EntityTypeBuilder<SiteRedirect> b)
    {
        b.ToTable("SiteRedirects");
        b.HasKey(x => x.Id);
        b.Property(x => x.FromPath).HasMaxLength(SiteRedirectRules.PathMaxLength);
        b.HasIndex(x => x.FromPath).IsUnique();
        b.Property(x => x.ToPath).HasMaxLength(SiteRedirectRules.PathMaxLength);
        b.HasIndex(x => x.ToPath);
        b.Property(x => x.Source).HasConversion<int>();
        b.Property(x => x.Note).HasMaxLength(SiteRedirectRules.NoteMaxLength);
        b.Property(x => x.UpdatedBy).HasMaxLength(ProductPageRules.UpdatedByMaxLength);
    }
}
