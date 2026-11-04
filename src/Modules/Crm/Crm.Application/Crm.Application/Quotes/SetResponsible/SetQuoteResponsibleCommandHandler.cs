using Crm.Application.Quotes.Access;
using Crm.Application.Quotes.Contracts;
using Crm.Application.Quotes.Mapping;
using Crm.Application.Quotes.Persistence;
using Crm.Application.Quotes.Staff;
using Crm.Domain.Quotes;

namespace Crm.Application.Quotes.SetResponsible;

public sealed class SetQuoteResponsibleCommandHandler(IQuoteStore store, IQuoteStaffReader staff, TimeProvider clock)
    : ICommandHandler<SetQuoteResponsibleCommand, Result<QuoteResponse>>
{
    public async ValueTask<Result<QuoteResponse>> Handle(SetQuoteResponsibleCommand command,
        CancellationToken cancellationToken)
    {
        if (!command.Actor.CanSeeAll) return Result.Forbidden(QuoteMessages.AssignForbidden);

        var quote = await store.LoadAsync(command.Id, cancellationToken);
        if (quote is null) return Result.NotFound(QuoteMessages.NotFound);
        if (quote.Version != command.Version) return Result.Conflict(QuoteMessages.Stale);
        if (quote.IsArchived) return Result.Conflict(QuoteMessages.Archived);

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
