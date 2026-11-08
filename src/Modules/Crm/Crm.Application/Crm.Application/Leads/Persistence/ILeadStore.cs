using Crm.Domain.Leads;
using Crm.Domain.Tasks;

namespace Crm.Application.Leads.Persistence;

/// <summary>Persistence port of leads; implemented by the Crm DbContext. Tasks are here for «Подзвонити» (R-08).</summary>
public interface ILeadStore
{
    DbSet<Lead> Leads { get; }
    DbSet<LeadCall> LeadCalls { get; }
    DbSet<LeadCallResult> LeadCallResults { get; }
    DbSet<LeadCategory> LeadCategories { get; }
    DbSet<LeadSyncState> LeadSyncStates { get; }
    DbSet<ManagerTask> ManagerTasks { get; }

    /// <summary>
    /// Opens a transaction holding the leads sync lock; null when another sync holds it (R-03). Disposing without
    /// commit rolls back.
    /// </summary>
    Task<ILeadSyncScope?> TryBeginSyncAsync(CancellationToken cancellationToken);

    /// <summary>Saves; a stale <see cref="Lead.Version"/> throws <see cref="DbUpdateConcurrencyException"/>.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

public interface ILeadSyncScope : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken);
}
