using Content.Application.Features.SeoOverview.Queries.GetSeoOverview;
using BuildingBlocks.Api.Errors;

namespace Content.Api.Endpoints.SeoOverview.GetSeoOverview;

internal static class GetSeoOverviewEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getSeoOverview")
            .WithSummary("Site-wide problems, counts per area and every page with at least one problem")
            .Produces<SeoOverviewResponse>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetSeoOverviewQuery(), cancellationToken)).ToHttpResult();
}
