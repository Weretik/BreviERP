using Ardalis.Specification;
using Crm.Application.Customers.Access;
using Crm.Application.Quotes.Pricing;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.CustomSewing.Versions;
using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Versions;

namespace Crm.Application.CustomSewing.ResolveQuoteVersions.Specifications;

public sealed class CustomQuoteVersionSpec : Specification<CustomSewingVersion, ResolvedCustomQuoteVersion>
{
    public CustomQuoteVersionSpec(IReadOnlyCollection<Guid> versionIds, Guid? quoteId, CrmActor actor,
        IQueryable<Quote> quotes, IQueryable<QuoteVersion> history)
    {
        var user = new CrmUserId(actor.UserId);
        Query.AsNoTracking().Where(x => versionIds.Contains(x.PublicId) && actor.UserId > 0 &&
            (actor.IsAdmin || (x.Draft.QuoteId == null && x.Draft.OwnerId == user)
                || (x.Draft.Quote != null && x.Draft.Quote.OwnerId == user)
                || quotes.Any(quote => quote.PublicId == quoteId && quote.OwnerId == user
                    && history.Any(version => version.QuoteId == quote.Id
                        && version.Items.Any(item => item.Variants.Any(variant => variant.CustomVersionId == x.Id))))));
        Query.Select(x => new ResolvedCustomQuoteVersion(x.Id, new QuoteCustomVersionSnapshot(x.Draft.PublicId,
            x.PublicId, x.Number, x.Configuration, x.Basis)));
    }
}
