using Crm.Domain.Quotes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Crm.Domain.Quotes.Items;

namespace Crm.Infrastructure.Persistence.Quotes;

public sealed class QuoteItemLineConfiguration : IEntityTypeConfiguration<QuoteItemLine>
{
    public void Configure(EntityTypeBuilder<QuoteItemLine> b)
    {
        b.ToTable("QuoteItemLines", t =>
        {
            t.HasCheckConstraint("CK_QuoteItemLines_Position", "\"Position\" >= 0");
            t.HasCheckConstraint("CK_QuoteItemLines_Quantity", "\"Quantity\" > 0");
            t.HasCheckConstraint("CK_QuoteItemLines_ManualMinutes",
                "\"ManualMinutes\" IS NULL OR (\"Kind\" = 'Operation' AND \"ManualMinutes\" > 0)");
            t.HasCheckConstraint("CK_QuoteItemLines_Reference",
                "(\"Kind\" = 'Service' AND \"ReferenceId\" = 0 AND \"CostCode\" IS NOT NULL) OR " +
                "(\"Kind\" <> 'Service' AND \"ReferenceId\" > 0 AND \"CostCode\" IS NULL)");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd();
        b.Property(x => x.Kind).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Name).HasMaxLength(QuoteLimits.NameMaxLength);
        b.Property(x => x.Unit).HasMaxLength(QuoteLimits.UnitMaxLength);
        b.Property(x => x.Quantity).HasPrecision(12, 4);
        b.Property(x => x.ManualMinutes).HasPrecision(12, 4);
        b.Property(x => x.CostCode).HasMaxLength(QuoteLimits.CostCodeMaxLength);
        b.HasIndex(x => new { x.ItemId, x.Position });
    }
}
