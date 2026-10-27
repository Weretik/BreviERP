using Crm.Domain.Quotes.Items;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations.Quotes;

public sealed class QuoteItemConfiguration : IEntityTypeConfiguration<QuoteItem>
{
    public void Configure(EntityTypeBuilder<QuoteItem> builder)
    {
        builder.ToTable("QuoteVersionItems");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Title).HasMaxLength(300).IsRequired();
        builder.Property(x => x.PublicDescription).HasMaxLength(5000);
        builder.Property(x => x.PublicComment).HasMaxLength(5000);
        builder.Property(x => x.InternalNote).HasMaxLength(5000);
        builder.HasMany(x => x.Variants).WithOne().HasForeignKey(x => x.ItemId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.VersionId, x.SortOrder }).IsUnique();
    }
}
