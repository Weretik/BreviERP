using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Purchasing.Domain.Sync;

namespace Purchasing.Infrastructure.Persistence.Sync;

public sealed class PurchaseSyncStateConfiguration : IEntityTypeConfiguration<PurchaseSyncState>
{
    public void Configure(EntityTypeBuilder<PurchaseSyncState> b)
    {
        b.ToTable("SyncStates");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasMaxLength(40);
        b.Property(x => x.Error).HasMaxLength(PurchaseSyncState.ErrorMaxLength);
    }
}
