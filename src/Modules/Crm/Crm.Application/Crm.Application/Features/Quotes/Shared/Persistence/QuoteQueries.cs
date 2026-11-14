using Crm.Domain.Quotes;
using Crm.Application.Abstractions.Quotes;

namespace Crm.Application.Features.Quotes.Shared.Persistence;

internal static class QuoteQueries
{
    public static Task<Quote?> LoadAsync(this IQuoteStore store, long id, CancellationToken cancellationToken) =>
        store.Quotes
            .Include(x => x.Items).ThenInclude(x => x.Options).ThenInclude(x => x.Fabrics)
            .Include(x => x.Items).ThenInclude(x => x.Lines)
            .AsSplitQuery()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
}
