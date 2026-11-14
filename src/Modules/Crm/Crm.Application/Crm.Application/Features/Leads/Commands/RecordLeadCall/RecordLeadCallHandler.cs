using Crm.Domain.Leads;
using Crm.Domain.Tasks;
using Crm.Application.Features.Leads.Shared;
using Crm.Application.Features.Leads.Shared.Access;
using Crm.Application.Features.Leads.Shared.Calling;
using Crm.Application.Features.Leads.Shared.Persistence;
using Crm.Application.Abstractions.Leads;
using Crm.Application.Abstractions.Tasks;

namespace Crm.Application.Features.Leads.Commands.RecordLeadCall;

/// <summary>
/// Records the call, moves the lead to the result's stage and keeps «Мої завдання» in step (R-08): the lead's open
/// «Подзвонити» task is marked done, and a next contact makes a new one for the actor on that time. A free lead is
/// taken by the caller first; somebody else's lead is refused.
/// </summary>
public sealed class RecordLeadCallHandler(ILeadStore store, ITaskStaffReader staff, TimeProvider clock)
    : ICommandHandler<RecordLeadCallCommand, Result<LeadDetailResponse>>
{
    private static readonly TimeSpan PastTolerance = TimeSpan.FromMinutes(5);

    public async ValueTask<Result<LeadDetailResponse>> Handle(RecordLeadCallCommand command,
        CancellationToken cancellationToken)
    {
        var input = command.Input;
        var lead = await store.LoadAsync(command.Id, cancellationToken);
        if (lead is null) return Result.NotFound(LeadMessages.NotFound);
        if (lead.Version != input.Version) return Result.Conflict(LeadMessages.Stale);
        if (!lead.IsOpen) return Result.Invalid(new ValidationError(LeadMessages.Closed));
        if (lead.OwnerUserId is not null && !lead.IsOwnedBy(command.Actor.UserId))
            return Result.Forbidden(LeadMessages.NotOwner);

        var result = await store.LeadCallResults.SingleOrDefaultAsync(x => x.Id == input.ResultId && x.IsActive,
            cancellationToken);
        if (result is null) return Result.Invalid(new ValidationError(LeadMessages.ResultNotFound));

        var now = clock.GetUtcNow();
        var next = NextContact(result, input.NextContactAt, now);
        if (result.Outcome.NeedsNextContact() && next is null)
            return Result.Invalid(new ValidationError(LeadMessages.NextContactRequired));
        if (next is { } due && due < now - PastTolerance)
            return Result.Invalid(new ValidationError(LeadMessages.NextContactPast));

        var member = await staff.FindAsync(command.Actor.UserId, cancellationToken);
        if (member is null) return Result.Forbidden(LeadMessages.ActorUnknown);
        var by = new LeadPerson(member.UserId, member.Name);
        var taskPerson = new ManagerTaskPerson(member.UserId, member.Name);

        lead.Take(by, now);
        var call = lead.RecordCall(by, result, input.Comment, next, now);

        foreach (var open in await store.OpenTasksAsync(lead.Id, cancellationToken))
            open.ChangeStatus(ManagerTaskStatus.Done, taskPerson, now);
        if (call.NextContactAt is { } nextAt)
            store.ManagerTasks.Add(LeadCallTask.Create(lead, call, taskPerson, nextAt, now));

        return await store.TrySaveAsync(cancellationToken)
            ? Result.Success(await store.DetailAsync(lead, now, cancellationToken))
            : Result.Conflict(LeadMessages.Stale);
    }

    /// <summary>A retry without a date is planned <see cref="LeadCallResult.RetryDays"/> days later (R-06).</summary>
    private static DateTimeOffset? NextContact(LeadCallResult result, DateTimeOffset? given, DateTimeOffset now) =>
        result.Outcome.Closes() ? null
        : given is { } date ? date.ToUniversalTime()
        : result.Outcome == LeadOutcome.Retry ? now.AddDays(result.RetryDays ?? 1)
        : null;
}
