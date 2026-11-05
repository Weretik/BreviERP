using Purchasing.Domain.Files;
using Purchasing.Domain.Messages;
using Purchasing.Domain.Sources;
using Purchasing.Domain.Sync;

namespace Purchasing.Application.Abstractions;

/// <summary>Persistence port of the Purchasing module; implemented by PurchasingDbContext.</summary>
public interface IPurchasingDbContext
{
    DbSet<PurchaseMessage> Messages { get; }
    DbSet<PurchaseSourceRow> SourceRows { get; }
    DbSet<PurchaseSourceRevision> SourceRevisions { get; }
    DbSet<PurchaseMessageFile> Files { get; }
    DbSet<PurchaseSyncState> SyncStates { get; }

    /// <summary>Opens the sync transaction under a lock; null when another sync holds the lock.</summary>
    Task<IPurchaseSyncScope?> TryBeginSyncAsync(CancellationToken cancellationToken);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
