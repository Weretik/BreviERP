using Ardalis.Specification;
using Crm.Application.Customers.Access;
using Crm.Application.Quotes.Contracts.Responses;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.Quotes;

namespace Crm.Application.Quotes.GetQuotes.Specifications;

public sealed class QuotePageSpec : Specification<Quote, QuoteSummary>
{
    public QuotePageSpec(CrmActor actor, string? search, string? state, bool? archived,
        int? ownerId, int page, int pageSize)
    {
        Query.AsNoTracking().Where(x => actor.IsAdmin || x.OwnerId == new CrmUserId(actor.UserId));
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToUpperInvariant();
            Query.Where(x => x.RecipientName.ToUpper().Contains(term) ||
                (x.Number != null && x.Number.ToUpper().Contains(term)));
        }
        if (state is not null) Query.Where(x => x.State == state);
        if (archived is not null) Query.Where(x => x.Archived == archived);
        if (ownerId is not null) Query.Where(x => x.OwnerId == new CrmUserId(ownerId.Value));
        Query.OrderByDescending(x => x.UpdatedAt).ThenBy(x => x.PublicId)
            .Skip((page - 1) * pageSize).Take(pageSize);
        Query.Select(x => new QuoteSummary(x.PublicId, x.RecipientName, x.Revision,
            x.CurrentVersionNumber, x.State, x.Archived, x.Number, x.OwnerId.Value, x.UpdatedAt));
    }
}
