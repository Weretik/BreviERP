using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Quotes.Contracts.Responses;
using Mediator;

namespace Crm.Application.Quotes.GetQuotes;

public sealed record GetQuotesQuery(
    CrmActor Actor, string? Search, string? State, bool? Archived,
    int? OwnerId, int Page, int PageSize)
    : IQuery<PagedResult<IReadOnlyList<QuoteSummary>>>;
