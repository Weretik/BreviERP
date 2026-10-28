using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Quotes.Contracts.Responses;
using Mediator;

namespace Crm.Application.Quotes.GetQuoteVersions;

public sealed record GetQuoteVersionsQuery(Guid QuoteId, int Page, int PageSize, CrmActor Actor)
    : IQuery<Result<PagedResult<IReadOnlyList<QuoteVersionSummary>>>>;
