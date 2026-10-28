using Ardalis.Specification;
using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Quotes.Contracts.Responses;
using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Versions;

namespace Crm.Application.Quotes.Persistence;

public interface IQuoteReadRepository : IReadRepositoryBase<Quote>
{
    Task<QuoteVersion?> FindVersionAsync(long id, CancellationToken cancellationToken);
    Task<QuoteVersion?> FindVersionAsync(Guid quoteId, Guid versionId, CrmActor actor, CancellationToken cancellationToken);
    Task<PagedResult<IReadOnlyList<QuoteVersionSummary>>> VersionsAsync(
        Guid quoteId, CrmActor actor, int page, int pageSize, CancellationToken cancellationToken);
    Task<PagedResult<IReadOnlyList<QuoteEventView>>> EventsAsync(
        Guid quoteId, CrmActor actor, int page, int pageSize, CancellationToken cancellationToken);
}
