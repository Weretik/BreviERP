using Crm.Application.Quotes.Access;
using Crm.Application.Quotes.Contracts;
using Crm.Application.Quotes.Introductions.Building;
using Crm.Application.Quotes.Introductions.Contracts;
using Crm.Application.Quotes.Mapping;
using Crm.Application.Quotes.Persistence;
using Crm.Application.Quotes.Staff;
using Crm.Domain.Quotes;

namespace Crm.Application.Quotes.Introductions.SaveIntroduction;

public sealed class SaveQuoteIntroductionCommandHandler(
    IQuoteStore store, IQuoteStaffReader staff, QuoteIntroductionBuilder builder, TimeProvider clock)
    : ICommandHandler<SaveQuoteIntroductionCommand, Result<QuoteResponse>>
{
    public async ValueTask<Result<QuoteResponse>> Handle(SaveQuoteIntroductionCommand command,
        CancellationToken cancellationToken)
    {
        var quote = await store.LoadAsync(command.Id, cancellationToken);
        if (quote is null || !command.Actor.CanAccess(quote)) return Result.NotFound(QuoteMessages.NotFound);
        if (quote.Kind != QuoteKind.Introduction) return Result.Conflict(QuoteMessages.WrongKind);
        if (quote.Version != command.Input.Version) return Result.Conflict(QuoteMessages.Stale);
        if (quote.IsArchived) return Result.Conflict(QuoteMessages.Archived);

        var actor = await staff.PersonAsync(command.Actor, cancellationToken);
        if (actor is null) return Result.Forbidden(QuoteMessages.ActorUnknown);

        var now = clock.GetUtcNow();
        var input = command.Input;
        var introduction = input.ToIntroduction();
        var items = await builder.BuildAsync(introduction, input.Pages, quote.Items, actor, now, cancellationToken);
        if (!items.IsSuccess) return items.Map(_ => default(QuoteResponse)!);

        quote.Update(input.ToHeader(), items.Value, now);
        quote.SetIntroduction(introduction);
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
