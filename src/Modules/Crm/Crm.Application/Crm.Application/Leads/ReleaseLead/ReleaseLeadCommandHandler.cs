using Crm.Application.Leads.Access;
using Crm.Application.Leads.Calling;
using Crm.Application.Leads.Contracts;
using Crm.Application.Leads.Persistence;

namespace Crm.Application.Leads.ReleaseLead;

/// <summary>The open «Подзвонити» task of the lead is deleted: nobody has to call any more (R-08).</summary>
public sealed class ReleaseLeadCommandHandler(ILeadStore store, TimeProvider clock)
    : ICommandHandler<ReleaseLeadCommand, Result<LeadDetailResponse>>
{
    public async ValueTask<Result<LeadDetailResponse>> Handle(ReleaseLeadCommand command,
        CancellationToken cancellationToken)
    {
        var lead = await store.LoadAsync(command.Id, cancellationToken);
        if (lead is null) return Result.NotFound(LeadMessages.NotFound);
        if (lead.Version != command.Version) return Result.Conflict(LeadMessages.Stale);
        var now = clock.GetUtcNow();
        if (lead.OwnerUserId is null) return Result.Success(await store.DetailAsync(lead, now, cancellationToken));
        if (!lead.IsOwnedBy(command.Actor.UserId) && !command.Actor.IsAdmin)
            return Result.Forbidden(LeadMessages.ReleaseForbidden);

        lead.Release(now);
        store.ManagerTasks.RemoveRange(await store.OpenTasksAsync(lead.Id, cancellationToken));
        return await store.TrySaveAsync(cancellationToken)
            ? Result.Success(await store.DetailAsync(lead, now, cancellationToken))
            : Result.Conflict(LeadMessages.Stale);
    }
}
