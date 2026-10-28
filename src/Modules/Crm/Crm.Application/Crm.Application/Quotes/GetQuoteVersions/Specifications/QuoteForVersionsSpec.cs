using Ardalis.Specification;
using Crm.Application.Customers.Access;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.Quotes;

namespace Crm.Application.Quotes.GetQuoteVersions.Specifications;

public sealed class QuoteForVersionsSpec : Specification<Quote>
{
    public QuoteForVersionsSpec(Guid quoteId, CrmActor actor)
    {
        Query.AsNoTracking().Where(x => x.PublicId == quoteId &&
            (actor.IsAdmin || x.OwnerId == new CrmUserId(actor.UserId)));
    }
}
