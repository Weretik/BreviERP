using Content.Application.Features.SitePages.Queries.GetSitePages;

namespace Content.Api.Endpoints.SitePages;

internal static class GetSitePagesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getSitePages");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetSitePagesQuery(), cancellationToken)).ToHttpResult();
}
