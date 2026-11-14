using Content.Application.Abstractions;
using Content.Application.Features.Redirects.Shared;

namespace Content.Application.Features.Redirects.Queries.GetSiteRedirects;

public sealed class GetSiteRedirectsHandler(IContentDbContext db)
    : IQueryHandler<GetSiteRedirectsQuery, Result<SiteRedirectList>>
{
    public async ValueTask<Result<SiteRedirectList>> Handle(GetSiteRedirectsQuery query, CancellationToken cancellationToken)
    {
        var rows = await db.SiteRedirects.AsNoTracking().OrderBy(x => x.FromPath).ToListAsync(cancellationToken);
        return new SiteRedirectList(rows.Select(RedirectMapping.Row).ToArray());
    }
}
