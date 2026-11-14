using Content.Application.Abstractions;
using Content.Application.Features.SiteCompany.Shared;

namespace Content.Application.Features.SiteCompany.Queries.GetSiteCompany;

public sealed class GetSiteCompanyHandler(IContentDbContext db)
    : IQueryHandler<GetSiteCompanyQuery, Result<SiteCompanyResponse>>
{
    public async ValueTask<Result<SiteCompanyResponse>> Handle(GetSiteCompanyQuery query,
        CancellationToken cancellationToken) =>
        SiteCompanyReads.Response(await db.CompanyInfo.AsNoTracking().FirstOrDefaultAsync(cancellationToken));
}
