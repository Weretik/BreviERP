using Crm.Application.CustomSewing.ResolveQuoteVersions.Specifications;
using Ardalis.Specification.EntityFrameworkCore;
using Crm.Application.Customers.Access;
using Crm.Application.Quotes.Pricing;
using Crm.Domain.Customers.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Persistence.CustomSewing.Reading;

public sealed class QuoteCustomVersionReader(CrmDbContext db) : IQuoteCustomVersionReader
{
    public async Task<ResolvedCustomQuoteVersion?> ReadAsync(Guid versionId, Guid? quoteId, CrmActor actor, CancellationToken ct)
        => (await ReadManyAsync([versionId], quoteId, actor, ct)).SingleOrDefault();

    public async Task<IReadOnlyList<ResolvedCustomQuoteVersion>> ReadManyAsync(IReadOnlyCollection<Guid> versionIds,
        Guid? quoteId, CrmActor actor, CancellationToken ct)
    {
        if (versionIds.Count == 0) return [];
        if (versionIds.Count > 3000) throw new ArgumentOutOfRangeException(nameof(versionIds));
        return await SpecificationEvaluator.Default.GetQuery(db.CustomVersions,
            new CustomQuoteVersionSpec(versionIds, quoteId, actor, db.Quotes, db.QuoteVersions)).ToArrayAsync(ct);
    }
}
