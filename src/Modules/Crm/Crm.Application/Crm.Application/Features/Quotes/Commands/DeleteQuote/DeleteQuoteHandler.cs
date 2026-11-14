using Crm.Application.Features.Quotes.Shared.Access;
using Crm.Application.Features.Quotes.Shared.Persistence;
using Crm.Application.Abstractions.Quotes;

namespace Crm.Application.Features.Quotes.Commands.DeleteQuote;

public sealed class DeleteQuoteHandler(IQuoteStore store, IQuoteProductionOrders production)
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
