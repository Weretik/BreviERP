using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Purchasing.Domain.Documents;
using Purchasing.Domain.Files;
using Purchasing.Domain.Messages;

namespace Purchasing.Infrastructure.Persistence.Configurations;

public sealed class PurchaseCheckedDocumentConfiguration : IEntityTypeConfiguration<PurchaseCheckedDocument>
{
    public void Configure(EntityTypeBuilder<PurchaseCheckedDocument> b)
    {
        b.ToTable("CheckedDocuments");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasMaxLength(64);
        b.Property(x => x.MessageId).HasMaxLength(64);
        b.Property(x => x.SourceUid).HasMaxLength(64);
        b.Property(x => x.PayloadHash).HasMaxLength(64);
        b.Property(x => x.Origin).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Vat).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Model).HasMaxLength(80);
        b.Property(x => x.CostUsd).HasPrecision(12, 6);
        b.Property(x => x.CheckedBy).HasMaxLength(PurchaseCheckedDocument.ByMaxLength);
        b.HasIndex(x => x.MessageId);
        b.HasIndex(x => x.SourceUid);
        b.HasIndex(x => x.DocumentDate);
        b.HasOne<PurchaseMessageFile>().WithOne().HasForeignKey<PurchaseCheckedDocument>(x => x.Id).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<PurchaseMessage>().WithMany().HasForeignKey(x => x.MessageId).OnDelete(DeleteBehavior.Restrict);
    }
}
