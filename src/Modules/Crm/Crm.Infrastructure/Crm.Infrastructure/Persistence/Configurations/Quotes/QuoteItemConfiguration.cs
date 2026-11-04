using Crm.Domain.Quotes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations.Quotes;

public sealed class QuoteItemConfiguration : IEntityTypeConfiguration<QuoteItem>
{
    public void Configure(EntityTypeBuilder<QuoteItem> b)
    {
        b.ToTable("QuoteItems", t =>
        {
            t.HasCheckConstraint("CK_QuoteItems_Position", "\"Position\" >= 0");
            t.HasCheckConstraint("CK_QuoteItems_Product",
                "(\"Kind\" = 'OneOffPpe' AND \"ProductId\" IS NULL) OR (\"Kind\" <> 'OneOffPpe' AND \"ProductId\" IS NOT NULL)");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd();
        b.Property(x => x.Kind).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Code).HasMaxLength(QuoteLimits.CodeMaxLength);
        b.Property(x => x.Name).HasMaxLength(QuoteLimits.NameMaxLength);
        b.Property(x => x.Subtitle).HasMaxLength(QuoteLimits.NameMaxLength);
        b.Property(x => x.Description).HasMaxLength(QuoteLimits.TextMaxLength);
        b.Property(x => x.SiteUrl).HasMaxLength(QuoteLimits.UrlMaxLength);
        b.Property(x => x.PhotoUrl).HasMaxLength(QuoteLimits.UrlMaxLength);
        b.Property(x => x.Season).HasMaxLength(QuoteLimits.CodeMaxLength);
        b.Property(x => x.SupplierUrl).HasMaxLength(QuoteLimits.UrlMaxLength);
        b.Ignore(x => x.ShowsPositivePrice);

        b.HasIndex(x => new { x.QuoteId, x.Position });
        b.HasIndex(x => x.ProductId);

        b.HasMany(x => x.Options).WithOne().HasForeignKey(x => x.ItemId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Options).HasField("_options").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
