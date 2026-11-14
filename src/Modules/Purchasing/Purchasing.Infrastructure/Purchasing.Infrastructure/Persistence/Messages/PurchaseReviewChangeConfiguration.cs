using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Purchasing.Domain.Messages;

namespace Purchasing.Infrastructure.Persistence.Messages;

public sealed class PurchaseReviewChangeConfiguration : IEntityTypeConfiguration<PurchaseReviewChange>
{
    public void Configure(EntityTypeBuilder<PurchaseReviewChange> b)
    {
        b.ToTable("ReviewChanges");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).UseIdentityByDefaultColumn();
        b.Property(x => x.MessageId).HasMaxLength(64);
        b.Property(x => x.Reviewer).HasMaxLength(PurchaseMessage.ReviewerMaxLength);
        b.ComplexProperty(x => x.Before, review => Configure(review, "Before"));
        b.ComplexProperty(x => x.After, review => Configure(review, "After"));
        b.HasIndex(x => new { x.MessageId, x.Id });
        b.HasOne<PurchaseMessage>().WithMany().HasForeignKey(x => x.MessageId).OnDelete(DeleteBehavior.Restrict);
    }

    private static void Configure(ComplexPropertyBuilder<PurchaseReview> review, string prefix)
    {
        review.Property(x => x.SupplierId).HasColumnName($"{prefix}SupplierId");
        review.Property(x => x.Note).HasColumnName($"{prefix}Note").HasMaxLength(PurchaseMessage.ReviewNoteMaxLength);
        review.Property(x => x.Version).HasColumnName($"{prefix}Version");
    }
}
