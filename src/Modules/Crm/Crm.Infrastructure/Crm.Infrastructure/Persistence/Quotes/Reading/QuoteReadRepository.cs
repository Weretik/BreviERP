using Ardalis.Specification.EntityFrameworkCore;
using Crm.Application.Quotes.Persistence;
using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Versions;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Persistence.Quotes.Reading;

public sealed partial class QuoteReadRepository(CrmDbContext db) : RepositoryBase<Quote>(db), IQuoteReadRepository
{
    public Task<QuoteVersion?> FindVersionAsync(long id, CancellationToken cancellationToken)
        => db.QuoteVersions.AsNoTracking().AsSplitQuery()
            .Include(x => x.Items).ThenInclude(x => x.Variants).ThenInclude(x => x.Tiers)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
}
