using Content.Application.Features.SitePages.Queries.GetPublicSitePages;

namespace Content.Api.Endpoints.PublicSitePages;

internal static class GetPublicSitePagesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getPublicSitePages");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetPublicSitePagesQuery(), cancellationToken)).ToHttpResult();
}
