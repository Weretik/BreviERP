using Reference.Application.Features.FabricColor.Queries.GetFabricColors;
using BuildingBlocks.Api.Errors;

namespace Reference.Api.Endpoints.FabricColors.GetFabricColors;

internal static class GetFabricColorsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getFabricColors")
            .WithSummary("List colours in list order")
            .WithDescription("Requires CatalogRead. Inactive colours are listed too; an empty list when there are none.").Produces<List<FabricColorRowDTO>>()
            .RequireAuthorization("CatalogRead");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetFabricColorsQuery(), cancellationToken)).ToHttpResult();
}
