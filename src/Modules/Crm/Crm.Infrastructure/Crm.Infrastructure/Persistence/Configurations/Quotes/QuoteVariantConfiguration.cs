using Crm.Domain.Quotes.Variants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations.Quotes;

public sealed class QuoteVariantConfiguration : IEntityTypeConfiguration<QuoteVariant>
{
    public void Configure(EntityTypeBuilder<QuoteVariant> builder)
    {
        builder.ToTable("QuoteItemVariants", table =>
        {
            table.HasCheckConstraint("CK_QuoteItemVariants_Kind", "\"Kind\" IN ('sewing','ppe','oneOffPpe','customSewing')");
            table.HasCheckConstraint("CK_QuoteItemVariants_CustomVersion", "(\"Kind\" = 'customSewing') = (\"CustomVersionId\" IS NOT NULL)");
        });
        builder.Property(x => x.CustomVersionId).HasConversion(x => x.HasValue ? x.Value.Value : (long?)null,
            x => x.HasValue ? new Crm.Domain.CustomSewing.Identifiers.CustomVersionId(x.Value) : null);
        builder.HasOne<Crm.Domain.CustomSewing.Versions.CustomSewingVersion>().WithMany()
            .HasForeignKey(x => x.CustomVersionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Title).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Kind).HasMaxLength(20).IsRequired();
        builder.Property(x => x.PriceBasisJson).HasColumnType("jsonb").IsRequired();
        builder.HasMany(x => x.Tiers).WithOne().HasForeignKey(x => x.VariantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.ItemId, x.SortOrder }).IsUnique();
    }
}
