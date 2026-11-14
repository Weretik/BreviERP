using Crm.Domain.Quotes;
using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Shared.Access;
using Crm.Application.Features.Quotes.Shared.Building;
using Crm.Application.Features.Quotes.Shared.Mapping;
using Crm.Application.Features.Quotes.Shared.Persistence;
using Crm.Application.Abstractions.Quotes;

namespace Crm.Application.Features.Quotes.Commands.SaveQuote;

public sealed class SaveQuoteHandler(
    IQuoteStore store, IQuoteStaffReader staff, QuoteItemsBuilder builder, TimeProvider clock)
    : ICommandHandler<SaveQuoteCommand, Result<QuoteResponse>>
{
    public async ValueTask<Result<QuoteResponse>> Handle(SaveQuoteCommand command, CancellationToken cancellationToken)
    {
        var quote = await store.LoadAsync(command.Id, cancellationToken);
        if (quote is null || !command.Actor.CanAccess(quote)) return Result.NotFound(QuoteMessages.NotFound);
        if (quote.Kind != QuoteKind.Request) return Result.Conflict(QuoteMessages.WrongKind);
        if (quote.Version != command.Input.Version) return Result.Conflict(QuoteMessages.Stale);

        var actor = await staff.PersonAsync(command.Actor, cancellationToken);
        if (actor is null) return Result.Forbidden(QuoteMessages.ActorUnknown);

        var now = clock.GetUtcNow();
        var items = await builder.BuildAsync(command.Input.Items, quote.Items, actor, now, cancellationToken);
        if (!items.IsSuccess) return items.Map(_ => default(QuoteResponse)!);

        var input = command.Input;
        var updated = quote.Update(new QuoteHeader(input.InternalName, input.CustomerId, input.RecipientName, input.CompanyName,
            input.ClientComment, input.ManagerNote), items.Value, now);
        if (updated.IsFailure) return updated.Error!.ToResult<QuoteResponse>();
        quote.Record(QuoteEventKind.Saved, actor, null, now);
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
