using Crm.Domain.SiteRequests;
using Crm.Application.Features.SiteRequests.Shared;

namespace Crm.Application.Features.SiteRequests.Queries.GetSiteRequests;

public sealed class GetSiteRequestsValidator : AbstractValidator<GetSiteRequestsQuery>
{
    public GetSiteRequestsValidator()
    {
        RuleFor(x => x.Scope).Must(x => x is null or "Open" or "Mine" or "Closed" or "All");
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
