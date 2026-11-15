using Content.Application.Features.SiteCompany.Shared.Public;
using Content.Application.Features.SiteCompany.Queries.GetPublicSiteCompany;
using BuildingBlocks.Api.Errors;

namespace Content.Api.Endpoints.PublicSiteCompany.GetPublicSiteCompany;

internal static class GetPublicSiteCompanyEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getPublicSiteCompany")
            .WithSummary("The facts; 404 before they exist, and the site then keeps its own")
            .Produces<PublicSiteCompanyResponse>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetPublicSiteCompanyQuery(), cancellationToken)).ToHttpResult();
}
