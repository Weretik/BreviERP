using Crm.Domain.Customers;
using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Versions;

namespace Crm.Application.Quotes.Persistence;

public interface IQuoteWriteSession
{
    Task<Customer?> FindCustomerAsync(Guid publicId, CancellationToken cancellationToken);
    Task<Quote?> FindQuoteAsync(Guid publicId, CancellationToken cancellationToken);
    Task<QuoteVersion?> FindVersionAsync(long id, CancellationToken cancellationToken);
    void Add(Quote quote);
    void Add(QuoteVersion version);
    Task FlushAsync(CancellationToken cancellationToken);
}
