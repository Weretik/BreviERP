using Crm.Domain.Customers;
using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Identifiers;
using Crm.Domain.Quotes.Versions;
using Crm.Domain.Quotes.Issues;

namespace Crm.Application.Quotes.Persistence;

public interface IQuoteWriteSession
{
    Task<Customer?> FindCustomerAsync(Guid publicId, CancellationToken cancellationToken);
    Task<Quote?> FindQuoteAsync(Guid publicId, CancellationToken cancellationToken);
    Task<QuoteVersion?> FindVersionAsync(long id, CancellationToken cancellationToken);
    Task<QuoteVersion?> FindVersionAsync(QuoteId quoteId, Guid publicId, CancellationToken cancellationToken);
    Task<ActiveDocumentVersions?> FindActiveDocumentVersionsAsync(CancellationToken cancellationToken);
    void Add(Quote quote);
    void Add(QuoteVersion version);
    void Add(QuoteIssue issue);
    Task FlushAsync(CancellationToken cancellationToken);
}
