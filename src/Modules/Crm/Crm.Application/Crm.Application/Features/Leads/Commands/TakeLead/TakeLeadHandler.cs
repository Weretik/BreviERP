using Crm.Domain.Leads;
using Crm.Application.Features.Leads.Shared;
using Crm.Application.Features.Leads.Shared.Access;
using Crm.Application.Features.Leads.Shared.Calling;
using Crm.Application.Features.Leads.Shared.Persistence;
using Crm.Application.Abstractions.Leads;
using Crm.Application.Abstractions.Tasks;
using Crm.Domain.Leads.ValueObjects;

namespace Crm.Application.Features.Leads.Commands.TakeLead;

public sealed class TakeLeadHandler(ILeadStore store, ITaskStaffReader staff, TimeProvider clock)
    : ICommandHandler<TakeLeadCommand, Result<LeadDetailResponse>>
{
    public async ValueTask<Result<LeadDetailResponse>> Handle(TakeLeadCommand command,
        CancellationToken cancellationToken)
    {
        var lead = await store.LoadAsync(command.Id, cancellationToken);
        if (lead is null) return Result.NotFound(LeadMessages.NotFound);
        if (!lead.IsOpen) return Result.Invalid(new ValidationError(LeadMessages.Closed));
        var member = await staff.FindAsync(command.Actor.UserId, cancellationToken);
        if (member is null) return Result.Forbidden(LeadMessages.ActorUnknown);

        var now = clock.GetUtcNow();
        if (!lead.Take(new LeadPerson(member.UserId, member.Name), now))
            return Result.Conflict(LeadMessages.TakenBy(lead.OwnerName));
        if (!await store.TrySaveAsync(cancellationToken))
        {
            var fresh = await store.Leads.AsNoTracking().Where(x => x.Id == command.Id).Select(x => x.OwnerName)
                .SingleOrDefaultAsync(cancellationToken);
            return Result.Conflict(LeadMessages.TakenBy(fresh));
        }

        return Result.Success(await store.DetailAsync(lead, now, cancellationToken));
    }
}
