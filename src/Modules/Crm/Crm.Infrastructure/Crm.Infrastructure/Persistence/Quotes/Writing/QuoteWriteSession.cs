using Crm.Application.Quotes.Persistence;
using Crm.Domain.Customers;
using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Versions;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Persistence.Quotes.Writing;

public sealed class QuoteWriteSession(CrmDbContext db) : IQuoteWriteSession
{
    public Task<Customer?> FindCustomerAsync(Guid publicId, CancellationToken cancellationToken)
        => db.Customers.Include(x => x.Contacts)
            .SingleOrDefaultAsync(x => x.PublicId == publicId, cancellationToken);

    public Task<Quote?> FindQuoteAsync(Guid publicId, CancellationToken cancellationToken)
        => db.Quotes.SingleOrDefaultAsync(x => x.PublicId == publicId, cancellationToken);

    public Task<QuoteVersion?> FindVersionAsync(long id, CancellationToken cancellationToken)
        => db.QuoteVersions.AsSplitQuery()
            .Include(x => x.Items).ThenInclude(x => x.Variants).ThenInclude(x => x.Tiers)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public void Add(Quote quote) => db.Quotes.Add(quote);
    public void Add(QuoteVersion version) => db.QuoteVersions.Add(version);
    public Task FlushAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
