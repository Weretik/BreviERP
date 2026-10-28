using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Quotes.Ownership;
using Mediator;

namespace Crm.Application.Quotes.GetQuoteAssignees;

public sealed record GetQuoteAssigneesQuery(string? Search, int Page, int PageSize, CrmActor Actor)
    : IQuery<Result<PagedResult<IReadOnlyList<QuoteAssignee>>>>;
