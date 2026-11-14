using Crm.Domain.Quotes;
using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Shared.Access;
using Crm.Application.Features.Quotes.Shared.Mapping;
using Crm.Application.Features.Quotes.Shared.Persistence;
using Crm.Application.Abstractions.Quotes;

namespace Crm.Application.Features.Quotes.Commands.SetQuoteResponsible;

public sealed class SetQuoteResponsibleHandler(IQuoteStore store, IQuoteStaffReader staff, TimeProvider clock)
    : ICommandHandler<SetQuoteResponsibleCommand, Result<QuoteResponse>>
{
    public async ValueTask<Result<QuoteResponse>> Handle(SetQuoteResponsibleCommand command,
        CancellationToken cancellationToken)
    {
        if (!command.Actor.CanSeeAll) return Result.Forbidden(QuoteMessages.AssignForbidden);

        var quote = await store.LoadAsync(command.Id, cancellationToken);
        if (quote is null) return Result.NotFound(QuoteMessages.NotFound);
        if (quote.Version != command.Version) return Result.Conflict(QuoteMessages.Stale);

        var member = await staff.FindAsync(command.UserId, cancellationToken);
        if (member is null) return Result.Invalid(new ValidationError(QuoteMessages.StaffNotFound));

        var actor = await staff.PersonAsync(command.Actor, cancellationToken);
        if (actor is null) return Result.Forbidden(QuoteMessages.ActorUnknown);

        quote.AssignResponsible(new QuotePerson(member.UserId, member.Name), actor, clock.GetUtcNow());
        try
        {
            await store.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Conflict(QuoteMessages.Stale);
        }

        return Result.Success(quote.ToResponse());
    }
}
