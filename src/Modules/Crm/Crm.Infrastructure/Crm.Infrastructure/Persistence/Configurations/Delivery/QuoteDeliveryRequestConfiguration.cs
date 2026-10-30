using Crm.Domain.Delivery;
using Crm.Domain.Quotes.Identifiers;
using Crm.Infrastructure.Persistence.Delivery;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations.Delivery;

public sealed class QuoteDeliveryRequestConfiguration : IEntityTypeConfiguration<QuoteDeliveryRequest>
{
    public void Configure(EntityTypeBuilder<QuoteDeliveryRequest> b)
    {
        b.ToTable("QuoteDeliveryRequests", "crm");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd();
        b.Property(x => x.DeliveryId).HasConversion(x => x.Value, x => new QuoteDeliveryId(x));
        b.Property(x => x.QuoteId).HasConversion(x => x.Value, x => new QuoteId(x));
        b.Property(x => x.IntentHash).HasMaxLength(64).IsRequired();
        b.HasIndex(x => new { x.ActorId, x.RequestId }).IsUnique();
        b.HasOne<QuoteDelivery>().WithMany().HasForeignKey(x => new { x.QuoteId, x.DeliveryId })
            .HasPrincipalKey(x => new { x.QuoteId, x.Id }).OnDelete(DeleteBehavior.Restrict);
    }
}
