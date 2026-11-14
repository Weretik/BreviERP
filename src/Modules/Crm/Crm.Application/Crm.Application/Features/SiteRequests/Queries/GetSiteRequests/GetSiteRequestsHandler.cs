using Crm.Domain.SiteRequests;
using Crm.Application.Features.SiteRequests.Shared;
using Crm.Application.Abstractions.SiteRequests;

namespace Crm.Application.Features.SiteRequests.Queries.GetSiteRequests;

public sealed class GetSiteRequestsHandler(ISiteRequestStore store)
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
