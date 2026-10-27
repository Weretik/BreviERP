using Ardalis.Specification;
using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Versions;

namespace Crm.Application.Quotes.Persistence;

public interface IQuoteReadRepository : IReadRepositoryBase<Quote>
{
    Task<QuoteVersion?> FindVersionAsync(long id, CancellationToken cancellationToken);
}
