using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Contracts;
using Mediator;
using Crm.Application.CustomSewing.Persistence;

namespace Crm.Application.CustomSewing.SaveDraft;

public sealed class SaveCustomDraftCommandHandler(ICustomMutationStore store)
    : ICommandHandler<SaveCustomDraftCommand, Result<CustomDetail>>
{
    public async ValueTask<Result<CustomDetail>> Handle(SaveCustomDraftCommand command, CancellationToken cancellationToken)
    {
        if (command.RequestId == Guid.Empty || command.Actor.UserId <= 0 || command.ExpectedRevision <= 0 || command.DraftId == Guid.Empty)
            return Result.Invalid(new ValidationError("request", "invalid_request"));
        return await store.ExecuteAsync("SaveCustomDraft", command.RequestId, command.Actor, command, async (session, ct) =>
        {
            if (command.Configuration is null) return Result.Invalid(new ValidationError("configuration", "required"));
            var draft = await session.FindDraftAsync(command.DraftId, command.Actor, ct);
            if (draft is null) return Result.NotFound();
            if (draft.Quote?.Archived == true) return Result.Conflict("quote_not_editable");
            draft.Revise(command.ExpectedRevision, command.Configuration);
            await session.FlushAsync(ct);
            return Result.Success(await session.DetailAsync(draft, ct));
        }, cancellationToken);
    }
}
