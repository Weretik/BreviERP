using Crm.Application.Embroidery.Persistence;
using Crm.Application.Quotes.Access;
using Crm.Application.Quotes.Contracts;
using Crm.Application.Quotes.Embroidery;
using Crm.Application.Quotes.Mapping;
using Crm.Application.Quotes.Persistence;
using Crm.Application.Quotes.Staff;
using Crm.Domain.Embroidery.Saved;
using Crm.Domain.Quotes;

namespace Crm.Application.Quotes.AddEmbroidery;

/// <summary>
/// «Додати в КП» (EMB-001 R-12): appends a saved embroidery calculation as one item at the end of a КП. It needs no
/// version: it adds and never overwrites, and an editor open elsewhere sees the change as a stale save.
/// </summary>
/// <param name="Level">«Опт», «Стандарт» or «Терміново»: the saved price level the client sees.</param>
public sealed record AddEmbroideryToQuoteCommand(QuoteActor Actor, long QuoteId, long CalculationId, string Level)
    : ICommand<Result<QuoteResponse>>;

public sealed class AddEmbroideryToQuoteCommandHandler(
    IQuoteStore quotes, IEmbroideryStore embroidery, IQuoteStaffReader staff, TimeProvider clock)
    : ICommandHandler<AddEmbroideryToQuoteCommand, Result<QuoteResponse>>
{
    public const string CalculationNotFound = "Збережений розрахунок вишивки не знайдено.";
    public const string LevelNotFound = "У збереженому розрахунку немає такої ціни.";
    public const string Full = "У КП вже максимум позицій.";

    public async ValueTask<Result<QuoteResponse>> Handle(AddEmbroideryToQuoteCommand command,
        CancellationToken cancellationToken)
    {
        var quote = await quotes.LoadAsync(command.QuoteId, cancellationToken);
        if (quote is null || !command.Actor.CanAccess(quote)) return Result.NotFound(QuoteMessages.NotFound);
        if (quote.Kind != QuoteKind.Request) return Result.Conflict(QuoteMessages.WrongKind);
        if (quote.Items.Count >= QuoteLimits.MaxItems) return Result.Conflict(Full);

        var actor = await staff.PersonAsync(command.Actor, cancellationToken);
        if (actor is null) return Result.Forbidden(QuoteMessages.ActorUnknown);

        var saved = await embroidery.EmbroideryCalculations.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == command.CalculationId, cancellationToken);
        if (saved is null) return Result.Invalid(new ValidationError(CalculationNotFound));
        var price = QuoteEmbroideryItems.Level(saved, command.Level);
        if (price is null || price.Unit <= 0) return Result.Invalid(new ValidationError(LevelNotFound));

        var now = clock.GetUtcNow();
        var position = quote.Items.Count == 0 ? 0 : quote.Items.Max(x => x.Position) + 1;
        quote.AddItem(QuoteEmbroideryItems.Create(position, saved, price), now);
        quote.Record(QuoteEventKind.EmbroideryAdded, actor,
            $"{EmbroiderySavedCalculation.NumberOf(saved.Id)}, {price.Name}", now);
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

public sealed class AddEmbroideryToQuoteCommandValidator : AbstractValidator<AddEmbroideryToQuoteCommand>
{
    public AddEmbroideryToQuoteCommandValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.QuoteId).GreaterThan(0);
        RuleFor(x => x.CalculationId).GreaterThan(0);
        RuleFor(x => x.Level).NotEmpty().MaximumLength(EmbroiderySavedCalculation.NameMaxLength);
    }
}
