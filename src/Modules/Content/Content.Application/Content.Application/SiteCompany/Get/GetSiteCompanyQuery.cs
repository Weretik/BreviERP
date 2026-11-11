using Content.Application.Abstractions;
using Content.Application.Contracts;
using Content.Application.SiteCompany.Shared;

namespace Content.Application.SiteCompany.Get;

/// <summary>The company facts; the migration seeds them, so revision 0 only appears in an empty database (CNT-009 SC-01).</summary>
public sealed record GetSiteCompanyQuery : IQuery<Result<SiteCompanyResponse>>;

public sealed class GetSiteCompanyQueryHandler(IContentDbContext db)
    : IQueryHandler<GetSiteCompanyQuery, Result<SiteCompanyResponse>>
{
    public async ValueTask<Result<SiteCompanyResponse>> Handle(GetSiteCompanyQuery query,
        CancellationToken cancellationToken) =>
        SiteCompanyReads.Response(await db.CompanyInfo.AsNoTracking().FirstOrDefaultAsync(cancellationToken));
}
