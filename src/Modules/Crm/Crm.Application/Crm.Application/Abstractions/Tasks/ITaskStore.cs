using Crm.Domain.Tasks;

namespace Crm.Application.Abstractions.Tasks;

/// <summary>Persistence port of manager tasks; implemented by the Crm DbContext.</summary>
public interface ITaskStore
{
    DbSet<ManagerTask> ManagerTasks { get; }

    /// <summary>The display name of an active Crm customer, or null.</summary>
    Task<string?> FindCustomerNameAsync(Guid publicId, CancellationToken cancellationToken);

    /// <summary>Saves; a stale <see cref="ManagerTask.Version"/> throws <see cref="DbUpdateConcurrencyException"/>.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
