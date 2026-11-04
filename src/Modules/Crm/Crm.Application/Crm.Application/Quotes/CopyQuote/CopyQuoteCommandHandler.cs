using Crm.Application.Quotes.Access;
using Crm.Application.Quotes.Building;
using Crm.Application.Quotes.Contracts;
using Crm.Application.Quotes.Mapping;
using Crm.Application.Quotes.Persistence;
using Crm.Application.Quotes.Staff;

namespace Crm.Application.Quotes.CopyQuote;

public sealed class CopyQuoteCommandHandler(
    IQuoteStore store, IQuoteStaffReader staff, QuoteNumbering numbering, TimeProvider clock)
    : ICommandHandler<CopyQuoteCommand, Result<QuoteResponse>>
{
    public async ValueTask<Result<QuoteResponse>> Handle(CopyQuoteCommand command, CancellationToken cancellationToken)
    {
        // Not tracked: a retry of the number clears the change tracker.
        var source = await store.Quotes.AsNoTracking()
            .Include(x => x.Items).ThenInclude(x => x.Options)
            .Include(x => x.Items).ThenInclude(x => x.Lines)
            .AsSplitQuery()
            .SingleOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (source is null || !command.Actor.CanAccess(source)) return Result.NotFound(QuoteMessages.NotFound);

        var author = await staff.PersonAsync(command.Actor, cancellationToken);
        if (author is null) return Result.Forbidden(QuoteMessages.ActorUnknown);

        var now = clock.GetUtcNow();
        var added = await numbering.AddAsync(now,
            number => Task.FromResult(Result.Success(source.CopyAs(number, author, now))), cancellationToken);
        return added.Map(quote => quote.ToResponse());
    }
}
