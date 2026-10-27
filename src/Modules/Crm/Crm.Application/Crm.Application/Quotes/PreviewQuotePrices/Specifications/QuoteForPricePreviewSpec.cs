using Ardalis.Specification;
using Crm.Application.Customers.Access;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.Quotes;

namespace Crm.Application.Quotes.PreviewQuotePrices.Specifications;

public sealed class QuoteForPricePreviewSpec : Specification<Quote>
{
    public QuoteForPricePreviewSpec(Guid publicId, CrmActor actor)
    {
        Query.AsNoTracking().Where(x => x.PublicId == publicId &&
            (actor.IsAdmin || x.OwnerId == new CrmUserId(actor.UserId)));
    }
}
