using Crm.Domain.Quotes;
using Crm.Application.Features.Quotes.Shared.Persistence;
using Crm.Application.Abstractions.Quotes;
using Crm.Domain.Quotes.ValueObjects;

namespace Crm.Application.Features.Quotes.Shared.Building;

/// <summary>
/// Adds a new КП with the next number «номер-місяць-рік» (R-03); the counter restarts every Kyiv month and a parallel
/// create retries with the next one. A repeat with the same request id returns the КП the first attempt saved.
/// </summary>
public sealed class QuoteNumbering(IQuoteStore store)
{
    private const int Attempts = 5;

    /// <param name="create">Builds the КП for the given number; called again on every retry.</param>
    public async Task<Result<Quote>> AddAsync(DateTimeOffset now, Guid? requestId,
        Func<QuoteNumber, Task<Result<Quote>>> create, CancellationToken cancellationToken)
    {
        if (await SavedAsync(requestId, cancellationToken) is { } saved) return Result.Success(saved);
        for (var attempt = 1; ; attempt++)
        {
            var built = await create(await NextAsync(now, cancellationToken));
            if (!built.IsSuccess) return built;

            store.Quotes.Add(built.Value);
            try
            {
                await store.SaveChangesAsync(cancellationToken);
                return built;
            }
            catch (DbUpdateException exception) when (attempt < Attempts && store.IsDuplicateNumber(exception))
            {
                store.Quotes.Entry(built.Value).Context.ChangeTracker.Clear();
            }
            catch (DbUpdateException) when (requestId is not null)
            {
                // The same press arrived twice at once: the other request saved the КП.
                store.Quotes.Entry(built.Value).Context.ChangeTracker.Clear();
                if (await SavedAsync(requestId, cancellationToken) is { } other) return Result.Success(other);
                throw;
            }
        }
    }

    private async Task<Quote?> SavedAsync(Guid? requestId, CancellationToken cancellationToken) =>
        requestId is { } id && await store.Quotes.AsNoTracking().Where(x => x.CreateRequestId == id)
            .Select(x => (long?)x.Id).SingleOrDefaultAsync(cancellationToken) is { } quoteId
            ? await store.LoadAsync(quoteId, cancellationToken)
            : null;

    private async Task<QuoteNumber> NextAsync(DateTimeOffset now, CancellationToken cancellationToken)
    {
        var date = QuoteClock.KyivDate(now);
        var last = await store.Quotes.AsNoTracking()
            .Where(x => x.NumberYear == date.Year && x.NumberMonth == date.Month)
            .MaxAsync(x => (int?)x.NumberSequence, cancellationToken);
        return new QuoteNumber(date.Year, date.Month, (last ?? 0) + 1);
    }
}
