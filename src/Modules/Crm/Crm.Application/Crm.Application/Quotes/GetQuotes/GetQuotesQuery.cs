using Crm.Application.Quotes.Access;
using Crm.Application.Quotes.Contracts;

namespace Crm.Application.Quotes.GetQuotes;

/// <summary>КП the actor may see, newest change first (R-02).</summary>
/// <param name="ResponsibleUserId">Director/Admin filter by manager; a Manager always gets only their own.</param>
public sealed record GetQuotesQuery(
    QuoteActor Actor, string? Search, string? Status, int? ResponsibleUserId, int Page = 1, int PageSize = 50)
    : IQuery<Result<QuotePageResponse>>;
