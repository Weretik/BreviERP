using Content.Application.Features.SitePages.Shared;
using Content.Application.Features.SitePages.Queries.GetPublicSitePages;
using BuildingBlocks.Api.Errors;

namespace Content.Api.Endpoints.PublicSitePages.GetPublicSitePages;

internal static class GetPublicSitePagesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getPublicSitePages")
            .WithSummary("The saved texts by page key; a page that is not listed, or an empty text, keeps the site's own text")
            .Produces<PublicSitePagesResponse>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetPublicSitePagesQuery(), cancellationToken)).ToHttpResult();
}
