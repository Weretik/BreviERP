using Crm.Application.SiteRequests.Abstractions;
using Crm.Application.SiteRequests.Contracts;
using Crm.Domain.SiteRequests;

namespace Crm.Application.SiteRequests.GetList;

/// <summary>«Заявки з сайту», newest first (LEAD-002 SC-03).</summary>
/// <param name="Scope">`Open` (default: new and in work), `Mine`, `Closed` or `All`.</param>
public sealed record GetSiteRequestsQuery(int UserId, string? Scope, int Page = 1, int PageSize = 25)
    : IQuery<Result<SiteRequestPageResponse>>;

public sealed class GetSiteRequestsQueryValidator : AbstractValidator<GetSiteRequestsQuery>
{
    public GetSiteRequestsQueryValidator()
    {
        RuleFor(x => x.Scope).Must(x => x is null or "Open" or "Mine" or "Closed" or "All");
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

public sealed class GetSiteRequestsQueryHandler(ISiteRequestStore store)
    : IQueryHandler<GetSiteRequestsQuery, Result<SiteRequestPageResponse>>
{
    public async ValueTask<Result<SiteRequestPageResponse>> Handle(GetSiteRequestsQuery query,
        CancellationToken cancellationToken)
    {
        var all = store.SiteRequests.AsNoTracking();
        var counts = new SiteRequestCountsResponse(
            await all.CountAsync(x => x.Status == SiteRequestStatus.New, cancellationToken),
            await all.CountAsync(x => x.Status == SiteRequestStatus.InWork, cancellationToken),
            await all.CountAsync(x => x.Status == SiteRequestStatus.InWork && x.OwnerUserId == query.UserId,
                cancellationToken),
            await all.CountAsync(x => x.Status == SiteRequestStatus.Done || x.Status == SiteRequestStatus.Spam,
                cancellationToken));

        var list = query.Scope switch
        {
            "Mine" => all.Where(x => x.OwnerUserId == query.UserId),
            "Closed" => all.Where(x => x.Status == SiteRequestStatus.Done || x.Status == SiteRequestStatus.Spam),
            "All" => all,
            _ => all.Where(x => x.Status == SiteRequestStatus.New || x.Status == SiteRequestStatus.InWork),
        };
        var total = await list.CountAsync(cancellationToken);
        var items = await list.OrderByDescending(x => x.Id)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .ToListAsync(cancellationToken);
        return Result.Success(new SiteRequestPageResponse(items.Select(SiteRequestMapper.ToResponse).ToArray(), total,
            query.Page, query.PageSize, counts));
    }
}
