using Crm.Domain.Leads;
using Crm.Domain.Tasks;
using Crm.Application.Abstractions.Leads;

namespace Crm.Application.Features.Leads.Shared.Persistence;

internal static class LeadQueries
{
    public static Task<Lead?> LoadAsync(this ILeadStore store, long id, CancellationToken cancellationToken) =>
        store.Leads.Include(x => x.Calls).AsSplitQuery().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    /// <summary>The open «Подзвонити» tasks of a lead (normally one).</summary>
    public static Task<List<ManagerTask>> OpenTasksAsync(this ILeadStore store, long leadId,
        CancellationToken cancellationToken) =>
        store.ManagerTasks.Where(x => x.LeadId == leadId && x.Status != ManagerTaskStatus.Done)
            .ToListAsync(cancellationToken);

    /// <summary>Saves and maps a stale version to false (409).</summary>
    public static async Task<bool> TrySaveAsync(this ILeadStore store, CancellationToken cancellationToken)
    {
        try
        {
            await store.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            return false;
        }
    }
}
