using Content.Application.Abstractions;
using Content.Application.Contracts;
using Content.Application.Redirects.Shared;

namespace Content.Application.Redirects.GetList;

/// <summary>Every redirect, by the old address (CNT-005 SC-01); the Admin searches and filters the list itself.</summary>
public sealed record GetSiteRedirectsQuery : IQuery<Result<SiteRedirectList>>;

public sealed class GetSiteRedirectsQueryHandler(IContentDbContext db)
    : IQueryHandler<GetSiteRedirectsQuery, Result<SiteRedirectList>>
{
    public async ValueTask<Result<SiteRedirectList>> Handle(GetSiteRedirectsQuery query, CancellationToken cancellationToken)
    {
        var rows = await db.SiteRedirects.AsNoTracking().OrderBy(x => x.FromPath).ToListAsync(cancellationToken);
        return new SiteRedirectList(rows.Select(RedirectMapping.Row).ToArray());
    }
}
