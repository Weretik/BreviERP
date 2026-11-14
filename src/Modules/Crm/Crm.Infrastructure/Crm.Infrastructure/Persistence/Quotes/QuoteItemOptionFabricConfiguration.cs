using Crm.Domain.Quotes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Crm.Domain.Quotes.Items;

namespace Crm.Infrastructure.Persistence.Quotes;

public sealed class QuoteItemOptionFabricConfiguration : IEntityTypeConfiguration<QuoteItemOptionFabric>
{
    public void Configure(EntityTypeBuilder<QuoteItemOptionFabric> b)
    {
        b.ToTable("QuoteItemOptionFabrics", t =>
        {
            t.HasCheckConstraint("CK_QuoteItemOptionFabrics_Position", "\"Position\" >= 0");
            t.HasCheckConstraint("CK_QuoteItemOptionFabrics_Meters", "\"Meters\" > 0");
            t.HasCheckConstraint("CK_QuoteItemOptionFabrics_Fabric", "\"FabricId\" > 0");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd();
        b.Property(x => x.Name).HasMaxLength(QuoteLimits.NameMaxLength);
        b.Property(x => x.Meters).HasPrecision(12, 4);
        b.HasIndex(x => new { x.OptionId, x.Position });
    }
}
