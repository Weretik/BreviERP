using System.Linq.Expressions;
using Crm.Application.Customers.Access;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.CustomSewing.Drafts;

namespace Crm.Application.CustomSewing.Access;

public static class CustomAccess
{
    public static Expression<Func<CustomSewingDraft, bool>> For(CrmActor actor) => x => actor.UserId > 0 &&
        (actor.IsAdmin || (x.QuoteId == null && x.OwnerId == new CrmUserId(actor.UserId))
            || (x.Quote != null && x.Quote.OwnerId == new CrmUserId(actor.UserId)));
    public static Expression<Func<Crm.Domain.CustomSewing.Versions.CustomSewingVersion, bool>> ForVersion(CrmActor actor)
        => x => actor.UserId > 0 && (actor.IsAdmin
            || (x.Draft.QuoteId == null && x.Draft.OwnerId == new CrmUserId(actor.UserId))
            || (x.Draft.Quote != null && x.Draft.Quote.OwnerId == new CrmUserId(actor.UserId)));
}
