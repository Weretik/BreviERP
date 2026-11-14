using Crm.Domain.Quotes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Quotes;

public sealed class QuoteEventConfiguration : IEntityTypeConfiguration<QuoteEvent>
{
    public void Configure(EntityTypeBuilder<QuoteEvent> b)
    {
        b.ToTable("QuoteEvents");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd();
        b.Property(x => x.Kind).HasConversion<string>().HasMaxLength(30);
        b.Property(x => x.ActorName).HasMaxLength(QuoteLimits.NameMaxLength);
        b.Property(x => x.Details).HasMaxLength(QuoteLimits.ReasonMaxLength);
        b.HasIndex(x => new { x.QuoteId, x.At });
    }
}
