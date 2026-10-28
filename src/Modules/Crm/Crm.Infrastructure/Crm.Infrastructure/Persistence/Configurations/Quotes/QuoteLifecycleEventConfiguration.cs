using Crm.Domain.Customers.Identifiers;
using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Events;
using Crm.Domain.Quotes.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations.Quotes;

public sealed class QuoteLifecycleEventConfiguration : IEntityTypeConfiguration<QuoteLifecycleEvent>
{
    public void Configure(EntityTypeBuilder<QuoteLifecycleEvent> builder)
    {
        builder.ToTable("QuoteLifecycleEvents", table =>
            table.HasCheckConstraint("CK_QuoteLifecycleEvents_Revision", "\"Revision\" > 0 AND \"VersionNumber\" > 0"));
        builder.HasKey(x => x.PublicId);
        builder.Property(x => x.PublicId).ValueGeneratedNever();
        builder.Property(x => x.QuoteId).HasConversion(x => x.Value, x => new QuoteId(x));
        builder.Property(x => x.ActorId).HasConversion(x => x.Value, x => new CrmUserId(x));
        builder.Property(x => x.OwnerId).HasConversion(x => x.Value, x => new CrmUserId(x));
        builder.Property(x => x.PreviousOwnerId).HasConversion(
            x => x.HasValue ? x.Value.Value : (int?)null,
            x => x.HasValue ? new CrmUserId(x.Value) : null);
        builder.Property(x => x.Action).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(1000);
        builder.HasOne<Quote>().WithMany(x => x.History).HasForeignKey(x => x.QuoteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.QuoteId, x.Revision }).IsUnique();
    }
}
