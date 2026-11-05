using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Purchasing.Domain.Messages;

namespace Purchasing.Infrastructure.Persistence.Configurations;

public sealed class PurchaseMessageConfiguration : IEntityTypeConfiguration<PurchaseMessage>
{
    public void Configure(EntityTypeBuilder<PurchaseMessage> b)
    {
        b.ToTable("Messages");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasMaxLength(64);
        // Source cells keep any length the sheet has: a long cell must not stop the sync.
        b.HasIndex(x => x.TelegramKey).IsUnique();
        b.Property(x => x.MessageDate).HasColumnType("timestamp without time zone");
        b.Property(x => x.PaymentState).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.ReviewNote).HasMaxLength(PurchaseMessage.ReviewNoteMaxLength);
        b.HasIndex(x => x.SupplierId);
        b.HasIndex(x => x.MessageDate);
    }
}
