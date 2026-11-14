using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Purchasing.Domain.Documents;
using Purchasing.Domain.Files;
using Purchasing.Domain.Messages;

namespace Purchasing.Infrastructure.Persistence.Documents;

public sealed class PurchaseCheckedDocumentHistoryConfiguration : IEntityTypeConfiguration<PurchaseCheckedDocumentHistory>
{
    public void Configure(EntityTypeBuilder<PurchaseCheckedDocumentHistory> b)
    {
        b.ToTable("CheckedDocumentHistory");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).UseIdentityByDefaultColumn();
        b.Property(x => x.FileId).HasMaxLength(64);
        b.Property(x => x.PayloadHash).HasMaxLength(64);
        b.Property(x => x.Origin).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.CheckedBy).HasMaxLength(PurchaseCheckedDocument.ByMaxLength);
        b.Property(x => x.ReplacedBy).HasMaxLength(PurchaseCheckedDocument.ByMaxLength);
        b.HasIndex(x => new { x.FileId, x.Id });
        b.HasOne<PurchaseMessageFile>().WithMany().HasForeignKey(x => x.FileId).OnDelete(DeleteBehavior.Restrict);
    }
}
