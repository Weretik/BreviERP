using Content.Application.Features.SitePages.Queries.GetSitePages;
using BuildingBlocks.Api.Errors;

namespace Content.Api.Endpoints.SitePages.GetSitePages;

internal static class GetSitePagesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getSitePages")
            .WithSummary("Every fixed page in menu order with its texts; revision 0 for a page never saved")
            .Produces<SitePageListResponse>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetSitePagesQuery(), cancellationToken)).ToHttpResult();
}
