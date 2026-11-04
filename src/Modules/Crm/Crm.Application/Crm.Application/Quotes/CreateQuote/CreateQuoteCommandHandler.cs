using Crm.Application.Quotes.Access;
using Crm.Application.Quotes.Building;
using Crm.Application.Quotes.Contracts;
using Crm.Application.Quotes.Mapping;
using Crm.Application.Quotes.Persistence;
using Crm.Application.Quotes.Staff;
using Crm.Domain.Quotes;

namespace Crm.Application.Quotes.CreateQuote;

public sealed class CreateQuoteCommandHandler(
    IQuoteStore store, IQuoteStaffReader staff, QuoteItemsBuilder builder, TimeProvider clock)
    : ICommandHandler<CreateQuoteCommand, Result<QuoteResponse>>
{
    private const int NumberAttempts = 5;

    public async ValueTask<Result<QuoteResponse>> Handle(CreateQuoteCommand command, CancellationToken cancellationToken)
    {
        var author = await staff.PersonAsync(command.Actor, cancellationToken);
        if (author is null) return Result.Forbidden(QuoteMessages.ActorUnknown);

        var now = clock.GetUtcNow();
        var items = await builder.BuildAsync(command.Input.Items, [], author, now, cancellationToken);
        if (!items.IsSuccess) return items.Map(_ => default(QuoteResponse)!);

        var input = command.Input;
        var header = new QuoteHeader(input.InternalName, input.CustomerId, input.RecipientName, input.CompanyName,
            input.ClientComment, input.ManagerNote);

        // R-04: «номер-місяць-рік», the counter restarts every Kyiv month; a parallel create retries with the next one.
        for (var attempt = 1; ; attempt++)
        {
            var number = await NextNumberAsync(now, cancellationToken);
            var quote = new Quote(number, QuoteKind.Request, author, header, items.Value, now);
            store.Quotes.Add(quote);
            try
            {
                await store.SaveChangesAsync(cancellationToken);
                return Result.Success(quote.ToResponse());
            }
            catch (DbUpdateException exception) when (attempt < NumberAttempts && store.IsDuplicateNumber(exception))
            {
                store.Quotes.Entry(quote).Context.ChangeTracker.Clear();
                items = await builder.BuildAsync(command.Input.Items, [], author, now, cancellationToken);
                if (!items.IsSuccess) return items.Map(_ => default(QuoteResponse)!);
            }
        }
    }

    private async Task<QuoteNumber> NextNumberAsync(DateTimeOffset now, CancellationToken cancellationToken)
    {
        var date = QuoteClock.KyivDate(now);
        var last = await store.Quotes.AsNoTracking()
            .Where(x => x.NumberYear == date.Year && x.NumberMonth == date.Month)
            .MaxAsync(x => (int?)x.NumberSequence, cancellationToken);
        return new QuoteNumber(date.Year, date.Month, (last ?? 0) + 1);
    }
}
