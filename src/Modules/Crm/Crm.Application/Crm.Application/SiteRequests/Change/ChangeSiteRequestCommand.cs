using Crm.Application.SiteRequests.Abstractions;
using Crm.Application.SiteRequests.Contracts;
using Crm.Application.Tasks.Staff;
using Crm.Domain.SiteRequests;

namespace Crm.Application.SiteRequests.Change;

/// <summary>
/// «Беру», «Готово», «Не заявка» or «Повернути в нові» (LEAD-002 SC-04). Anyone with the page takes a free request;
/// only its owner or an Admin closes or frees it.
/// </summary>
public sealed record ChangeSiteRequestCommand(int UserId, bool IsAdmin, long Id, ChangeSiteRequestInput Input)
    : ICommand<Result<SiteRequestResponse>>;

public sealed class ChangeSiteRequestCommandValidator : AbstractValidator<ChangeSiteRequestCommand>
{
    public ChangeSiteRequestCommandValidator()
    {
        RuleFor(x => x.Input).NotNull();
        RuleFor(x => x.Input.Action).Must(x => x is "Take" or "Done" or "Spam" or "Reopen")
            .WithMessage(SiteRequestMessages.Action).When(x => x.Input is not null);
        RuleFor(x => x.Input.Note).MaximumLength(SiteRequest.NoteMaxLength).WithMessage(SiteRequestMessages.Note)
            .When(x => x.Input is not null);
    }
}

public sealed class ChangeSiteRequestCommandHandler(ISiteRequestStore store, ITaskStaffReader staff, TimeProvider clock)
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
                request.Close(command.Input.Action == "Done" ? SiteRequestStatus.Done : SiteRequestStatus.Spam,
                    member.UserId, member.Name, command.Input.Note, now);
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
