using Content.Application.Abstractions;
using Content.Domain.SitePages;
using Content.Application.Features.SitePages.Shared;

namespace Content.Application.Features.SitePages.Queries.GetSitePages;

/// <summary>Every fixed page of the site in menu order with its search texts (CNT-007 SC-01).</summary>
public sealed record GetSitePagesQuery : IQuery<Result<SitePageListResponse>>;

public sealed class GetSitePagesHandler(IContentDbContext db)
    : IQueryHandler<GetSitePagesQuery, Result<SitePageListResponse>>
{
    public async ValueTask<Result<SitePageListResponse>> Handle(GetSitePagesQuery query,
        CancellationToken cancellationToken)
    {
        var saved = await db.SitePages.AsNoTracking().ToDictionaryAsync(x => x.Key, StringComparer.Ordinal,
            cancellationToken);
        return new SitePageListResponse(SitePageCatalog.Pages
            .Select(entry => SitePageMapping.Response(entry, saved.GetValueOrDefault(entry.Key)))
            .ToList());
    }
}
