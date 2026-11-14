using Crm.Application.Features.Leads.Shared;
using Crm.Application.Features.Leads.Shared.Access;
using Crm.Application.Features.Leads.Shared.Calling;
using Crm.Application.Features.Leads.Shared.Persistence;
using Crm.Application.Abstractions.Leads;

namespace Crm.Application.Features.Leads.Commands.ReleaseLead;

/// <summary>The open «Подзвонити» task of the lead is deleted: nobody has to call any more (R-08).</summary>
public sealed class ReleaseLeadHandler(ILeadStore store, TimeProvider clock)
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
