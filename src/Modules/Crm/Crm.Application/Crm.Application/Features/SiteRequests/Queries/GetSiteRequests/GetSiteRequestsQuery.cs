using Crm.Domain.SiteRequests;
using Crm.Application.Features.SiteRequests.Shared;

namespace Crm.Application.Features.SiteRequests.Queries.GetSiteRequests;

/// <summary>«Заявки з сайту», newest first (LEAD-002 SC-03).</summary>
/// <param name="Scope">`Open` (default: new and in work), `Mine`, `Closed` or `All`.</param>
public sealed record GetSiteRequestsQuery(int UserId, string? Scope, int Page = 1, int PageSize = 25)
    : IQuery<Result<SiteRequestPageResponse>>;
