using Crm.Application.Quotes.Access;
using Crm.Application.Quotes.Contracts;

namespace Crm.Application.Quotes.GetQuotes;

/// <summary>КП the actor may see, newest change first (R-02).</summary>
/// <param name="ResponsibleUserId">Director/Admin filter by manager; a Manager always gets only their own.</param>
/// <param name="Archived">True lists only «Архів», false only КП in work (R-23).</param>
/// <param name="CustomerId">The client card tab «КП»: only this client's КП, in work and archived together.</param>
public sealed record GetQuotesQuery(
    QuoteActor Actor, string? Search, string? Status, int? ResponsibleUserId, int Page = 1, int PageSize = 50,
    bool Archived = false, long? CustomerId = null)
    : IQuery<Result<QuotePageResponse>>;
