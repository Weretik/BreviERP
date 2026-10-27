using Crm.Domain.Quotes.Pricing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations.Quotes;

public sealed class QuoteTierConfiguration : IEntityTypeConfiguration<QuoteTier>
{
    public void Configure(EntityTypeBuilder<QuoteTier> builder)
    {
        builder.ToTable("QuoteVariantTiers", table =>
            table.HasCheckConstraint("CK_QuoteVariantTiers_Amount", "\"Amount\" IS NULL OR \"Amount\" >= 0"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Amount).HasPrecision(20, 2);
        builder.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        builder.HasIndex(x => new { x.VariantId, x.Code }).IsUnique();
    }
}
