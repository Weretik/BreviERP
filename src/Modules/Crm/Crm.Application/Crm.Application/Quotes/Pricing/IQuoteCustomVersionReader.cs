using Crm.Application.Customers.Access;

namespace Crm.Application.Quotes.Pricing;

public interface IQuoteCustomVersionReader
{
    Task<IReadOnlyList<ResolvedCustomQuoteVersion>> ReadManyAsync(IReadOnlyCollection<Guid> versionIds, Guid? quoteId, CrmActor actor, CancellationToken cancellationToken);
    Task<ResolvedCustomQuoteVersion?> ReadAsync(Guid versionId, Guid? quoteId, CrmActor actor, CancellationToken cancellationToken);
}
