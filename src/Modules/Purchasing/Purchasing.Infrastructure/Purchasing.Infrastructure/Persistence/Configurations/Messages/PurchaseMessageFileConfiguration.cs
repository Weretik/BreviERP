using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Purchasing.Domain.Files;
using Purchasing.Domain.Messages;


namespace Purchasing.Infrastructure.Persistence.Configurations.Messages;

public sealed class PurchaseMessageFileConfiguration : IEntityTypeConfiguration<PurchaseMessageFile>
{
    public void Configure(EntityTypeBuilder<PurchaseMessageFile> b)
    {
        b.ToTable("MessageFiles");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasMaxLength(64);
        b.Property(x => x.MessageId).HasMaxLength(64);
        b.HasIndex(x => new { x.MessageId, x.Url }).IsUnique();
        b.Property(x => x.State).HasMaxLength(40);
        b.Property(x => x.Issue).HasMaxLength(1_000);
        b.HasOne<PurchaseMessage>().WithMany().HasForeignKey(x => x.MessageId).OnDelete(DeleteBehavior.Restrict);
    }
}
