using Crm.Domain.SiteRequests;
using Crm.Application.Features.SiteRequests.Shared;
using Crm.Application.Abstractions.SiteRequests;
using Crm.Application.Abstractions.Tasks;
using BuildingBlocks.Application;

namespace Crm.Application.Features.SiteRequests.Commands.ChangeSiteRequest;

public sealed class ChangeSiteRequestHandler(ISiteRequestStore store, ITaskStaffReader staff, TimeProvider clock)
    : ICommandHandler<ChangeSiteRequestCommand, Result<SiteRequestResponse>>
{
    public async ValueTask<Result<SiteRequestResponse>> Handle(ChangeSiteRequestCommand command,
        CancellationToken cancellationToken)
    {
        var request = await store.SiteRequests.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (request is null) return Result.NotFound(SiteRequestMessages.NotFound);
        if (request.Version != command.Input.Version) return Result.Conflict(SiteRequestMessages.Stale);
        var member = await staff.FindAsync(command.UserId, cancellationToken);
        if (member is null) return Result.Forbidden(SiteRequestMessages.ActorUnknown);

        var now = clock.GetUtcNow();
        var mayManage = command.IsAdmin || request.OwnerUserId is null || request.OwnerUserId == command.UserId;
        switch (command.Input.Action)
        {
            case "Take":
                if (!request.IsOpen) return Result.Invalid(new ValidationError(SiteRequestMessages.Closed));
                if (!request.Take(member.UserId, member.Name, now))
                    return Result.Conflict(SiteRequestMessages.TakenBy(request.OwnerName));
                break;
            case "Done" or "Spam":
                if (!request.IsOpen) return Result.Invalid(new ValidationError(SiteRequestMessages.Closed));
                if (!mayManage) return Result.Forbidden(SiteRequestMessages.NotYours);
                var closed = request.Close(command.Input.Action == "Done" ? SiteRequestStatus.Done : SiteRequestStatus.Spam,
                    member.UserId, member.Name, command.Input.Note, now);
                if (closed.IsFailure) return closed.Error!.ToResult<SiteRequestResponse>();
                break;
            default:
                if (!mayManage) return Result.Forbidden(SiteRequestMessages.NotYours);
                request.Reopen(now);
                break;
        }

        try
        {
            await store.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Conflict(SiteRequestMessages.Stale);
        }

        return Result.Success(SiteRequestMapper.ToResponse(request));
    }
}
