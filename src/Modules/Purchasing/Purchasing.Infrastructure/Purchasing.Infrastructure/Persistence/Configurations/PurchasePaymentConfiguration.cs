using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Purchasing.Domain.Messages;

namespace Purchasing.Infrastructure.Persistence.Configurations;

public sealed class PurchasePaymentConfiguration : IEntityTypeConfiguration<PurchasePayment>
{
    public void Configure(EntityTypeBuilder<PurchasePayment> b)
    {
        b.ToTable("Payments");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).UseIdentityByDefaultColumn();
        b.Property(x => x.MessageId).HasMaxLength(64);
        b.Property(x => x.LinkedBy).HasMaxLength(PurchaseMessage.ReviewerMaxLength);
        b.Property(x => x.UnlinkedBy).HasMaxLength(PurchaseMessage.ReviewerMaxLength);
        b.Ignore(x => x.Active);
        b.HasIndex(x => new { x.MessageId, x.Id });
        b.HasIndex(x => x.TransactionId);
        // One active link of an operation per invoice; removed links stay as history.
        b.HasIndex(x => new { x.MessageId, x.TransactionId }).IsUnique().HasFilter("\"UnlinkedAt\" IS NULL");
        b.HasOne<PurchaseMessage>().WithMany().HasForeignKey(x => x.MessageId).OnDelete(DeleteBehavior.Restrict);
    }
}
