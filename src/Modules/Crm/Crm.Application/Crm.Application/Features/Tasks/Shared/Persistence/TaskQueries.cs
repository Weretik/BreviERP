using Crm.Domain.Tasks;
using Crm.Application.Features.Tasks.Shared.Access;
using Crm.Application.Abstractions.Tasks;

namespace Crm.Application.Features.Tasks.Shared.Persistence;

internal static class TaskQueries
{
    public static Task<ManagerTask?> LoadAsync(this ITaskStore store, long id, CancellationToken cancellationToken) =>
        store.ManagerTasks
            .Include(x => x.Steps)
            .Include(x => x.Events)
            .AsSplitQuery()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    /// <summary>Tasks the actor may see; Director/Admin may narrow to one manager.</summary>
    public static IQueryable<ManagerTask> Visible(this ITaskStore store, TaskActor actor, int? assigneeUserId)
    {
        var tasks = store.ManagerTasks.AsNoTracking();
        if (!actor.IsAdmin) return tasks.Where(x => x.AssigneeUserId == actor.UserId);
        return assigneeUserId is { } assignee ? tasks.Where(x => x.AssigneeUserId == assignee) : tasks;
    }

    /// <summary>Saves and maps a stale version to 409.</summary>
    public static async Task<bool> TrySaveAsync(this ITaskStore store, CancellationToken cancellationToken)
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
