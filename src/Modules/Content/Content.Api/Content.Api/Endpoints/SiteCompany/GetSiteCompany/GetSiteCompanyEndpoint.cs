using Content.Application.Features.SiteCompany.Queries.GetSiteCompany;

namespace Content.Api.Endpoints.SiteCompany.GetSiteCompany;

internal static class GetSiteCompanyEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getSiteCompany");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetSiteCompanyQuery(), cancellationToken)).ToHttpResult();
}
