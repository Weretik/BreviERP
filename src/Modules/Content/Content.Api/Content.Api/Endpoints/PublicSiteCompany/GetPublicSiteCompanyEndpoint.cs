using Content.Application.Features.SiteCompany.Queries.GetPublicSiteCompany;

namespace Content.Api.Endpoints.PublicSiteCompany;

internal static class GetPublicSiteCompanyEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getPublicSiteCompany");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetPublicSiteCompanyQuery(), cancellationToken)).ToHttpResult();
}
