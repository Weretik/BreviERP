using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Shared.Access;
using Crm.Application.Features.Quotes.Shared.Persistence;
using Crm.Application.Abstractions.Quotes;

namespace Crm.Application.Features.Quotes.Commands.PurgeLegacyArchivedQuotes;

public sealed class PurgeLegacyArchivedQuotesHandler(
    IQuoteStore store, IQuoteProductionOrders production, IQuoteStaffReader staff, TimeProvider clock)
    : ICommandHandler<PurgeLegacyArchivedQuotesCommand, Result<LegacyArchivePurgeResponse>>
{
    public async ValueTask<Result<LegacyArchivePurgeResponse>> Handle(PurgeLegacyArchivedQuotesCommand command,
        CancellationToken cancellationToken)
    {
        if (!command.Actor.CanDelete) return Result<LegacyArchivePurgeResponse>.Forbidden(QuoteMessages.DeleteForbidden);
        var person = await staff.PersonAsync(command.Actor, cancellationToken);
        if (person is null) return Result<LegacyArchivePurgeResponse>.Conflict(QuoteMessages.ActorUnknown);

        var quotes = await store.Quotes.Where(x => x.ArchivedAt != null).ToListAsync(cancellationToken);
        var kept = await production.WithOrdersAsync(quotes.Select(x => x.Id).ToArray(), cancellationToken);
        var now = clock.GetUtcNow();
        foreach (var quote in quotes)
        {
            if (kept.Contains(quote.Id)) quote.ReturnFromLegacyArchive(person, now);
            else store.Quotes.Remove(quote);
        }

        try
        {
            await store.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<LegacyArchivePurgeResponse>.Conflict(QuoteMessages.Stale);
        }

        var returned = quotes.Count(x => kept.Contains(x.Id));
        return Result.Success(new LegacyArchivePurgeResponse(quotes.Count - returned, returned));
    }
}
