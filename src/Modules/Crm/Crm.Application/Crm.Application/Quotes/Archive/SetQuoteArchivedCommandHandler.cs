using Crm.Application.Quotes.Access;
using Crm.Application.Quotes.Contracts;
using Crm.Application.Quotes.Mapping;
using Crm.Application.Quotes.Persistence;
using Crm.Application.Quotes.Staff;

namespace Crm.Application.Quotes.Archive;

public sealed class SetQuoteArchivedCommandHandler(IQuoteStore store, IQuoteStaffReader staff, TimeProvider clock)
    : ICommandHandler<SetQuoteArchivedCommand, Result<QuoteResponse>>
{
    public async ValueTask<Result<QuoteResponse>> Handle(SetQuoteArchivedCommand command,
        CancellationToken cancellationToken)
    {
        var quote = await store.LoadAsync(command.Id, cancellationToken);
        if (quote is null || !command.Actor.CanAccess(quote)) return Result.NotFound(QuoteMessages.NotFound);
        if (quote.Version != command.Version) return Result.Conflict(QuoteMessages.Stale);

        var actor = await staff.PersonAsync(command.Actor, cancellationToken);
        if (actor is null) return Result.Forbidden(QuoteMessages.ActorUnknown);

        var now = clock.GetUtcNow();
        if (command.Archived) quote.Archive(actor, now);
        else quote.Restore(actor, now);
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
