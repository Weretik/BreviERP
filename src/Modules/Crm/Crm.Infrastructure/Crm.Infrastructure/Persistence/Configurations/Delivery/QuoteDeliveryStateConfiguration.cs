using Crm.Domain.Delivery;
using Crm.Infrastructure.Persistence.Delivery;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations.Delivery;

public sealed class QuoteDeliveryStateConfiguration : IEntityTypeConfiguration<QuoteDeliveryState>
{
    public void Configure(EntityTypeBuilder<QuoteDeliveryState> b)
    {
        b.ToTable("QuoteDeliveryStates", "crm", t =>
        {
            t.HasCheckConstraint("CK_QuoteDeliveryStates_Status", "\"Status\" IN ('queued','sending','reconciling','unknown','succeeded','failed')");
            t.HasCheckConstraint("CK_QuoteDeliveryStates_Lease", "(\"Status\" IN ('sending','reconciling') AND \"LeaseToken\" IS NOT NULL AND \"LeaseUntil\" IS NOT NULL) OR (\"Status\" NOT IN ('sending','reconciling') AND \"LeaseToken\" IS NULL AND \"LeaseUntil\" IS NULL)");
            t.HasCheckConstraint("CK_QuoteDeliveryStates_Attempt", "\"Attempt\" >= 0");
        });
        b.HasKey(x => x.DeliveryId);
        b.Property(x => x.DeliveryId).HasConversion(x => x.Value, x => new QuoteDeliveryId(x)).ValueGeneratedNever();
        b.Property(x => x.Status).HasMaxLength(20).IsRequired().IsConcurrencyToken();
        b.Property(x => x.LeaseToken).IsConcurrencyToken();
        b.Property(x => x.ErrorCode).HasMaxLength(80);
        b.Property(x => x.ProviderMessageId).HasMaxLength(200);
        b.HasOne<QuoteDelivery>().WithOne().HasForeignKey<QuoteDeliveryState>(x => x.DeliveryId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.Status, x.LeaseUntil, x.NextCheckAt, x.UpdatedAt });
    }
}
