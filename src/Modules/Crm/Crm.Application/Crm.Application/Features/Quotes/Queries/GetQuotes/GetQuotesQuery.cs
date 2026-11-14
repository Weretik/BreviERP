using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Shared.Access;

namespace Crm.Application.Features.Quotes.Queries.GetQuotes;

/// <summary>КП the actor may see, newest change first (R-02).</summary>
/// <param name="ResponsibleUserId">Director/Admin filter by manager; a Manager always gets only their own.</param>
/// <param name="CustomerId">The client card tab «КП»: only this client's КП.</param>
/// <remarks>A КП left from the removed «Архів» is never listed; the Director deletes those (R-23).</remarks>
public sealed record GetQuotesQuery(
    QuoteActor Actor, string? Search, string? Status, int? ResponsibleUserId, int Page = 1, int PageSize = 50,
    long? CustomerId = null)
    : IQuery<Result<QuotePageResponse>>;
