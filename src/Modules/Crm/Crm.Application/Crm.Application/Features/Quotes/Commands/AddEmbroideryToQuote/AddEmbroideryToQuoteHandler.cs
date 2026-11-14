using Crm.Domain.Embroidery.Saved;
using Crm.Domain.Embroidery.Settings;
using Crm.Domain.Quotes;
using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Shared.Access;
using Crm.Application.Features.Quotes.Shared.Embroidery;
using Crm.Application.Features.Quotes.Shared.Mapping;
using Crm.Application.Features.Quotes.Shared.Persistence;
using Crm.Application.Abstractions.Embroidery;
using Crm.Application.Abstractions.Quotes;
using Crm.Domain.Quotes.Enums;

namespace Crm.Application.Features.Quotes.Commands.AddEmbroideryToQuote;

public sealed class AddEmbroideryToQuoteHandler(
    IQuoteStore quotes, IEmbroideryStore embroidery, IQuoteStaffReader staff, TimeProvider clock)
    : ICommandHandler<AddEmbroideryToQuoteCommand, Result<QuoteResponse>>
{
    public const string CalculationNotFound = "Збережений розрахунок вишивки не знайдено.";
    public const string LevelNotFound = "У збереженому розрахунку немає ціни.";
    public const string Full = "У КП вже максимум позицій.";

    public async ValueTask<Result<QuoteResponse>> Handle(AddEmbroideryToQuoteCommand command,
        CancellationToken cancellationToken)
    {
        var quote = await quotes.LoadAsync(command.QuoteId, cancellationToken);
        if (quote is null || !command.Actor.CanAccess(quote)) return Result.NotFound(QuoteMessages.NotFound);
        if (quote.Kind != QuoteKind.Request) return Result.Conflict(QuoteMessages.WrongKind);

        var actor = await staff.PersonAsync(command.Actor, cancellationToken);
        if (actor is null) return Result.Forbidden(QuoteMessages.ActorUnknown);

        var saved = await embroidery.EmbroideryCalculations.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == command.CalculationId, cancellationToken);
        if (saved is null) return Result.Invalid(new ValidationError(CalculationNotFound));
        var price = saved.Price;
        if (price is null || price.Unit <= 0) return Result.Invalid(new ValidationError(LevelNotFound));

        var now = clock.GetUtcNow();
        var position = quote.Items.Count == 0 ? 0 : quote.Items.Max(x => x.Position) + 1;
        var added = quote.AddItem(QuoteEmbroideryItems.Create(position, saved, price), now);
        if (added.IsFailure) return added.Error!.ToResult<QuoteResponse>();
        quote.Record(QuoteEventKind.EmbroideryAdded, actor,
            $"{EmbroiderySavedCalculation.NumberOf(saved.Id)}, {EmbroideryCategories.Label(price.Name)}", now);
        try
        {
            await quotes.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Conflict(QuoteMessages.Stale);
        }

        return Result.Success(quote.ToResponse());
    }
}
