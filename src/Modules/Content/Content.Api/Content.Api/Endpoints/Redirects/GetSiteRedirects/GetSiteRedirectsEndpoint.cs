using Content.Application.Features.Redirects.Queries.GetSiteRedirects;
using BuildingBlocks.Api.Errors;

namespace Content.Api.Endpoints.Redirects.GetSiteRedirects;

internal static class GetSiteRedirectsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getSiteRedirects");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetSiteRedirectsQuery(), cancellationToken)).ToHttpResult();
}
