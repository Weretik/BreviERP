using Crm.Application.Customers.Access;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.CustomSewing.Versions;
using Crm.Infrastructure.Persistence;

namespace Crm.Infrastructure.Files.Access;

internal static class CustomFileVersionScope
{
    internal static IQueryable<CustomSewingVersion> Query(CrmDbContext db, CrmActor actor, Guid? quoteId)
    {
        var user = new CrmUserId(actor.UserId);
        var query = db.CustomVersions.Where(x => actor.UserId > 0);
        if (quoteId is { } quote)
            return query.Where(x => db.Quotes.Any(q => q.PublicId == quote && (actor.IsAdmin || q.OwnerId == user)
                && db.QuoteVersions.Any(v => v.QuoteId == q.Id
                    && v.Items.Any(item => item.Variants.Any(variant => variant.CustomVersionId == x.Id)))));
        return query.Where(x => actor.IsAdmin || (x.Draft.QuoteId == null && x.Draft.OwnerId == user)
            || (x.Draft.Quote != null && x.Draft.Quote.OwnerId == user));
    }
}
