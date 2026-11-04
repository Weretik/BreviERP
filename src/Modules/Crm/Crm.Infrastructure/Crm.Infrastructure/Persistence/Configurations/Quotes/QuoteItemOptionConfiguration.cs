using Crm.Domain.Quotes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations.Quotes;

public sealed class QuoteItemOptionConfiguration : IEntityTypeConfiguration<QuoteItemOption>
{
    public void Configure(EntityTypeBuilder<QuoteItemOption> b)
    {
        b.ToTable("QuoteItemOptions", t =>
        {
            t.HasCheckConstraint("CK_QuoteItemOptions_Position", "\"Position\" >= 0");
            foreach (var tier in new[] { "Retail", "Wholesale", "Large" })
                t.HasCheckConstraint($"CK_QuoteItemOptions_{tier}",
                    $"(\"{tier}Source\" IS NULL OR \"{tier}Source\" >= 0) AND (\"{tier}Manual\" IS NULL OR \"{tier}Manual\" >= 0)");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd();
        b.Property(x => x.Title).HasMaxLength(QuoteLimits.NameMaxLength);
        b.Property(x => x.Details).HasMaxLength(QuoteLimits.TextMaxLength);
        b.Property(x => x.ManualReason).HasMaxLength(QuoteLimits.ReasonMaxLength);
        b.Property(x => x.ManualByName).HasMaxLength(QuoteLimits.NameMaxLength);
        b.Property(x => x.FabricMeters).HasPrecision(12, 4);
        Price(b, x => x.Retail, "Retail");
        Price(b, x => x.Wholesale, "Wholesale");
        Price(b, x => x.Large, "Large");
        b.Ignore(x => x.Prices);
        b.Ignore(x => x.HasManualPrice);
        b.Ignore(x => x.ShowsPositivePrice);

        b.HasIndex(x => new { x.ItemId, x.Position });
        b.HasIndex(x => x.ArticleId);
    }

    private static void Price(EntityTypeBuilder<QuoteItemOption> b,
        System.Linq.Expressions.Expression<Func<QuoteItemOption, QuotePrice>> property, string tier) =>
        b.ComplexProperty(property, price =>
        {
            price.Property(x => x.Source).HasColumnName($"{tier}Source").HasPrecision(12, 2);
            price.Property(x => x.Manual).HasColumnName($"{tier}Manual").HasPrecision(12, 2);
            price.Property(x => x.Shown).HasColumnName($"{tier}Shown");
        });
}
