using Crm.Application.Quotes.Access;
using Crm.Application.Quotes.Persistence;
using Crm.Domain.Quotes;

namespace Crm.Application.Quotes.DeleteQuote;

public sealed class DeleteQuoteCommandHandler(IQuoteStore store) : ICommandHandler<DeleteQuoteCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteQuoteCommand command, CancellationToken cancellationToken)
    {
        var quote = await store.LoadAsync(command.Id, cancellationToken);
        if (quote is null || !command.Actor.CanAccess(quote)) return Result.NotFound(QuoteMessages.NotFound);
        if (quote.Version != command.Version) return Result.Conflict(QuoteMessages.Stale);
        if (quote.Status != QuoteStatus.Draft) return Result.Conflict(QuoteMessages.OnlyDraftDeleted);

        store.Quotes.Remove(quote);
        try
        {
            await store.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Conflict(QuoteMessages.Stale);
        }

        return Result.NoContent();
    }
}
