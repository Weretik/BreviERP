using Crm.Domain.Quotes;
using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Shared.Access;
using Crm.Application.Features.Quotes.Shared.Introductions;
using Crm.Application.Features.Quotes.Shared.Introductions.Building;
using Crm.Application.Features.Quotes.Shared.Mapping;
using Crm.Application.Features.Quotes.Shared.Persistence;
using Crm.Application.Abstractions.Quotes;

namespace Crm.Application.Features.Quotes.Commands.SaveQuoteIntroduction;

public sealed class SaveQuoteIntroductionHandler(
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

        var actor = await staff.PersonAsync(command.Actor, cancellationToken);
        if (actor is null) return Result.Forbidden(QuoteMessages.ActorUnknown);

        var now = clock.GetUtcNow();
        var input = command.Input;
        var introduction = input.ToIntroduction();
        var items = await builder.BuildAsync(introduction, input.Pages, quote.Items, actor, now, cancellationToken);
        if (!items.IsSuccess) return items.Map(_ => default(QuoteResponse)!);

        var updated = quote.Update(input.ToHeader(), items.Value, now);
        if (updated.IsFailure) return updated.Error!.ToResult<QuoteResponse>();
        var introduced = quote.SetIntroduction(introduction);
        if (introduced.IsFailure) return introduced.Error!.ToResult<QuoteResponse>();
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
