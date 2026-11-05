using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Purchasing.Domain.Documents;
using Purchasing.Domain.Files;
using Purchasing.Domain.Messages;

namespace Purchasing.Infrastructure.Persistence.Configurations;

public sealed class PurchaseRecognitionAttemptConfiguration : IEntityTypeConfiguration<PurchaseRecognitionAttempt>
{
    public void Configure(EntityTypeBuilder<PurchaseRecognitionAttempt> b)
    {
        b.ToTable("RecognitionAttempts");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).UseIdentityByDefaultColumn();
        b.Property(x => x.FileId).HasMaxLength(64);
        b.Property(x => x.MessageId).HasMaxLength(64);
        b.Property(x => x.Outcome).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Model).HasMaxLength(80);
        b.Property(x => x.CostUsd).HasPrecision(12, 6);
        b.Property(x => x.By).HasMaxLength(PurchaseCheckedDocument.ByMaxLength);
        b.HasIndex(x => new { x.FileId, x.Id });
        b.HasOne<PurchaseMessageFile>().WithMany().HasForeignKey(x => x.FileId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<PurchaseMessage>().WithMany().HasForeignKey(x => x.MessageId).OnDelete(DeleteBehavior.Restrict);
    }
}
