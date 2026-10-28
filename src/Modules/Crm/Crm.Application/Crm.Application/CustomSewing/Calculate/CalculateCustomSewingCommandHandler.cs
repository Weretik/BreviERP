using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Contracts;
using Mediator;
using Crm.Application.CustomSewing.Persistence;
using Crm.Application.CustomSewing.Pricing;

namespace Crm.Application.CustomSewing.Calculate;

public sealed class CalculateCustomSewingCommandHandler(ICustomMutationStore store, ICustomSewingPricing pricing)
    : ICommandHandler<CalculateCustomSewingCommand, Result<CustomDetail>>
{
    public async ValueTask<Result<CustomDetail>> Handle(CalculateCustomSewingCommand command, CancellationToken cancellationToken)
    {
        if (command.RequestId == Guid.Empty || command.Actor.UserId <= 0 || command.ExpectedRevision <= 0 || command.DraftId == Guid.Empty)
            return Result.Invalid(new ValidationError("request", "invalid_request"));
        return await store.ExecuteAsync("CalculateCustomSewing", command.RequestId, command.Actor, command, async (session, ct) =>
        {
            var draft = await session.FindDraftAsync(command.DraftId, command.Actor, ct);
            if (draft is null) return Result.NotFound();
            if (draft.Revision != command.ExpectedRevision || draft.Quote?.Archived == true)
                return Result.Conflict("revision_conflict");
            var basis = await pricing.CalculateAsync(draft.Configuration, DateTimeOffset.UtcNow, ct);
            session.AddBasis(draft, command.Actor.UserId, basis);
            await session.FlushAsync(ct);
            var detail = await session.DetailAsync(draft, ct);
            return Result.Success(detail with { Preview = basis });
        }, cancellationToken);
    }
}
