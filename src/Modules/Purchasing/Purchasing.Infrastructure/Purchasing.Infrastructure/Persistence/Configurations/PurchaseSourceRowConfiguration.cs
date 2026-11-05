using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Purchasing.Domain.Messages;
using Purchasing.Domain.Sources;

namespace Purchasing.Infrastructure.Persistence.Configurations;

public sealed class PurchaseSourceRowConfiguration : IEntityTypeConfiguration<PurchaseSourceRow>
{
    public void Configure(EntityTypeBuilder<PurchaseSourceRow> b)
    {
        b.ToTable("SourceRows");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasMaxLength(64);
        b.Property(x => x.MessageId).HasMaxLength(64);
        b.HasIndex(x => x.SourceUid).IsUnique();
        b.HasIndex(x => x.MessageId);
        b.HasOne<PurchaseMessage>().WithMany().HasForeignKey(x => x.MessageId).OnDelete(DeleteBehavior.Restrict);
    }
}
