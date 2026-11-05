using Microsoft.EntityFrameworkCore;
using Purchasing.Application.Abstractions;
using Purchasing.Domain.Files;
using Purchasing.Domain.Messages;
using Purchasing.Domain.Sources;
using Purchasing.Domain.Sync;

namespace Purchasing.Infrastructure.Persistence;

public sealed class PurchasingDbContext(DbContextOptions<PurchasingDbContext> options) : DbContext(options), IPurchasingDbContext
{
    public const string Schema = "purchasing";

    /// <summary>pg advisory lock key of register writes: the sheet sync and manual reviews ("PURCH" in ASCII digits, unique among Brevi locks).</summary>
    private const long WriteLockKey = 8_085_826_772;

    public DbSet<PurchaseMessage> Messages => Set<PurchaseMessage>();
    public DbSet<PurchaseSourceRow> SourceRows => Set<PurchaseSourceRow>();
    public DbSet<PurchaseSourceRevision> SourceRevisions => Set<PurchaseSourceRevision>();
    public DbSet<PurchaseMessageFile> Files => Set<PurchaseMessageFile>();
    public DbSet<PurchaseSyncState> SyncStates => Set<PurchaseSyncState>();
    public DbSet<PurchaseReviewChange> ReviewChanges => Set<PurchaseReviewChange>();

    public async Task<IPurchaseWriteScope?> TryBeginWriteAsync(CancellationToken cancellationToken)
    {
        var transaction = await Database.BeginTransactionAsync(cancellationToken);
        var locked = await Database
            .SqlQuery<bool>($"SELECT pg_try_advisory_xact_lock({WriteLockKey}) AS \"Value\"")
            .SingleAsync(cancellationToken);
        if (locked) return new PurchaseWriteScope(transaction);
        await transaction.DisposeAsync();
        return null;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema(Schema);
        builder.ApplyConfigurationsFromAssembly(typeof(PurchasingDbContext).Assembly);
    }
}
