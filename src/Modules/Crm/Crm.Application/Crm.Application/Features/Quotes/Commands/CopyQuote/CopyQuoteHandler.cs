using BuildingBlocks.Application.Idempotency;
using BuildingBlocks.Domain.Identifiers.Orders;
using Crm.Domain.Quotes;
using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Shared.Access;
using Crm.Application.Features.Quotes.Shared.Building;
using Crm.Application.Features.Quotes.Shared.Mapping;
using Crm.Application.Features.Quotes.Shared.Persistence;
using Crm.Application.Abstractions.Quotes;

namespace Crm.Application.Features.Quotes.Commands.CopyQuote;

public sealed class CopyQuoteHandler(
    IQuoteStore store, IQuoteStaffReader staff, QuoteNumbering numbering, TimeProvider clock)
    : ICommandHandler<CopyQuoteCommand, Result<QuoteResponse>>
{
    public async ValueTask<Result<QuoteResponse>> Handle(CopyQuoteCommand command, CancellationToken cancellationToken)
    {
        // Not tracked: a retry of the number clears the change tracker.
        var source = await store.Quotes.AsNoTracking()
            .Include(x => x.Items).ThenInclude(x => x.Options).ThenInclude(x => x.Fabrics)
            .Include(x => x.Items).ThenInclude(x => x.Lines)
            .AsSplitQuery()
            .SingleOrDefaultAsync(x => x.Id == new QuoteId(command.Id), cancellationToken);
        if (source is null || !command.Actor.CanAccess(source)) return Result.NotFound(QuoteMessages.NotFound);

        var author = await staff.PersonAsync(command.Actor, cancellationToken);
        if (author is null) return Result.Forbidden(QuoteMessages.ActorUnknown);

        var now = clock.GetUtcNow();
        var request = RequestKeys.Of(command.Input?.RequestId, command with { Input = command.Input is null ? null : command.Input with { RequestId = null } });
        var added = await numbering.AddAsync(now, request,
            number => Task.FromResult(Result.Success(source.CopyAs(number, author, now, request))), cancellationToken);
        return added.Map(quote => quote.ToResponse());
    }
}
