using Crm.Domain.Delivery;
using Crm.Domain.Quotes.Identifiers;
using Crm.Infrastructure.Persistence.Delivery;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations.Delivery;

public sealed class QuoteDeliveryEventConfiguration : IEntityTypeConfiguration<QuoteDeliveryEvent>
{
    public void Configure(EntityTypeBuilder<QuoteDeliveryEvent> b)
    {
        b.ToTable("QuoteDeliveryEvents", "crm", t => t.HasCheckConstraint("CK_QuoteDeliveryEvents_Kind", "\"Kind\" IN ('queued','submit_started','reconcile_started','succeeded','failed','unknown','cancelled')"));
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd();
        b.Property(x => x.DeliveryId).HasConversion(x => x.Value, x => new QuoteDeliveryId(x));
        b.Property(x => x.QuoteId).HasConversion(x => x.Value, x => new QuoteId(x));
        b.Property(x => x.Kind).HasMaxLength(30).IsRequired();
        b.Property(x => x.ErrorCode).HasMaxLength(80);
        b.Property(x => x.ProviderMessageId).HasMaxLength(200);
        b.HasOne<QuoteDelivery>().WithMany().HasForeignKey(x => new { x.QuoteId, x.QuoteVersionId, x.DeliveryId })
            .HasPrincipalKey(x => new { x.QuoteId, x.QuoteVersionId, x.Id }).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.DeliveryId, x.CreatedAt });
        b.HasIndex(x => new { x.DeliveryId, x.Kind }).IsUnique().HasFilter("\"Kind\" = 'succeeded'");
    }
}
