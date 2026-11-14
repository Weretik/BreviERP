using Reference.Application.Features.FabricColor.Queries.GetFabricColors;

namespace Reference.Api.Endpoints.FabricColors.GetFabricColors;

internal static class GetFabricColorsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getFabricColors").Produces<List<FabricColorRowDTO>>()
            .RequireAuthorization("CatalogRead");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetFabricColorsQuery(), cancellationToken)).ToHttpResult();
}
