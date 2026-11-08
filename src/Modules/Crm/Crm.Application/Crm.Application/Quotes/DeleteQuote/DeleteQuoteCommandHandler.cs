using Crm.Application.Quotes.Access;
using Crm.Application.Quotes.Persistence;
using Crm.Application.Quotes.ProductionLinks;

namespace Crm.Application.Quotes.DeleteQuote;

public sealed class DeleteQuoteCommandHandler(IQuoteStore store, IQuoteProductionOrders production)
    : ICommandHandler<DeleteQuoteCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteQuoteCommand command, CancellationToken cancellationToken)
    {
        if (!command.Actor.CanDelete) return Result.Forbidden(QuoteMessages.DeleteForbidden);
        var quote = await store.LoadAsync(command.Id, cancellationToken);
        if (quote is null || !command.Actor.CanAccess(quote)) return Result.NotFound(QuoteMessages.NotFound);
        if (quote.Version != command.Version) return Result.Conflict(QuoteMessages.Stale);
        var withOrders = await production.WithOrdersAsync([quote.Id], cancellationToken);
        if (withOrders.Contains(quote.Id)) return Result.Conflict(QuoteMessages.HasProductionOrder);

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
