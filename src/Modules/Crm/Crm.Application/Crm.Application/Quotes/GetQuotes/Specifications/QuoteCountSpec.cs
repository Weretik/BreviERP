using Ardalis.Specification;
using Crm.Application.Customers.Access;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.Quotes;

namespace Crm.Application.Quotes.GetQuotes.Specifications;

public sealed class QuoteCountSpec : Specification<Quote>
{
    public QuoteCountSpec(CrmActor actor, string? search, string? state, bool? archived, int? ownerId,
        DateTimeOffset? createdFrom = null, DateTimeOffset? createdBefore = null, Guid? sourceQuoteId = null)
    {
        createdFrom = createdFrom?.ToUniversalTime();
        createdBefore = createdBefore?.ToUniversalTime();
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
        if (createdFrom is not null) Query.Where(x => x.CreatedAt >= createdFrom);
        if (createdBefore is not null) Query.Where(x => x.CreatedAt < createdBefore);
        if (sourceQuoteId is not null) Query.Where(x => x.SourceQuotePublicId == sourceQuoteId);
    }
}
