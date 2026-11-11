using Content.Application.Abstractions;
using Content.Application.Contracts;
using Content.Application.SitePages.Shared;
using Content.Domain.SitePages;

namespace Content.Application.SitePages.GetList;

/// <summary>Every fixed page of the site in menu order with its search texts (CNT-007 SC-01).</summary>
public sealed record GetSitePagesQuery : IQuery<Result<SitePageListResponse>>;

public sealed class GetSitePagesQueryHandler(IContentDbContext db)
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
