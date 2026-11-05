using Purchasing.Domain.Documents;
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
    DbSet<PurchaseReviewChange> ReviewChanges { get; }
    DbSet<PurchaseCheckedDocument> CheckedDocuments { get; }
    DbSet<PurchaseCheckedDocumentHistory> CheckedDocumentHistory { get; }
    DbSet<PurchaseRecognitionAttempt> RecognitionAttempts { get; }

    /// <summary>
    /// Opens a register write transaction under the module lock; null while another write (usually the sheet sync)
    /// holds it. The sync and manual reviews never interleave.
    /// </summary>
    Task<IPurchaseWriteScope?> TryBeginWriteAsync(CancellationToken cancellationToken);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
