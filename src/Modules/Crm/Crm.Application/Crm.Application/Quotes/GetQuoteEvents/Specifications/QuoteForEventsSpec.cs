using Ardalis.Specification;
using Crm.Application.Customers.Access;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.Quotes;

namespace Crm.Application.Quotes.GetQuoteEvents.Specifications;

public sealed class QuoteForEventsSpec : Specification<Quote>
{
    public QuoteForEventsSpec(Guid quoteId, CrmActor actor)
    {
        Query.AsNoTracking().Where(x => x.PublicId == quoteId &&
            (actor.IsAdmin || x.OwnerId == new CrmUserId(actor.UserId)));
    }
}
