using BuildingBlocks.Domain;
using BuildingBlocks.Application.Idempotency;
using Crm.Domain.Quotes;
using Crm.Application.Features.Quotes.Shared.Persistence;
using Crm.Application.Abstractions.Quotes;
using Crm.Domain.Quotes.ValueObjects;

namespace Crm.Application.Features.Quotes.Shared.Building;

/// <summary>
/// Adds a new КП with the next number «номер-місяць-рік» (R-03); the counter restarts every Kyiv month and a parallel
/// create retries with the next one. A repeat with the same request id returns the КП the first attempt saved; the same id with other data answers 409.
/// </summary>
public sealed class QuoteNumbering(IQuoteStore store)
{
    private const int Attempts = 5;

    /// <param name="create">Builds the КП for the given number; called again on every retry.</param>
    public async Task<Result<Quote>> AddAsync(DateTimeOffset now, RequestKey? request,
        Func<QuoteNumber, Task<Result<Quote>>> create, CancellationToken cancellationToken)
    {
        if (await SavedAsync(request, cancellationToken) is { } saved) return saved;
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
            catch (DbUpdateException) when (request is not null)
            {
                // The same press arrived twice at once: the other request saved the КП.
                store.Quotes.Entry(built.Value).Context.ChangeTracker.Clear();
                if (await SavedAsync(request, cancellationToken) is { } other) return other;
                throw;
            }
        }
    }

    /// <summary>The КП saved under the request id, a conflict when the id was used for other data, or null.</summary>
    private async Task<Result<Quote>?> SavedAsync(RequestKey? request, CancellationToken cancellationToken)
    {
        if (request is not { } key) return null;
        var saved = await store.Quotes.AsNoTracking().Where(x => x.CreateRequestId == key.Id)
            .Select(x => new { Id = x.Id.Value, x.CreateRequestHash }).SingleOrDefaultAsync(cancellationToken);
        if (saved is null) return null;
        if (!key.Matches(saved.CreateRequestHash)) return RequestKeys.Conflict<Quote>();
        return await store.LoadAsync(saved.Id, cancellationToken) is { } quote ? Result.Success(quote) : null;
    }

    private async Task<QuoteNumber> NextAsync(DateTimeOffset now, CancellationToken cancellationToken)
    {
        var date = QuoteClock.KyivDate(now);
        var last = await store.Quotes.AsNoTracking()
            .Where(x => x.NumberYear == date.Year && x.NumberMonth == date.Month)
            .MaxAsync(x => (int?)x.NumberSequence, cancellationToken);
        return new QuoteNumber(date.Year, date.Month, (last ?? 0) + 1);
    }
}
