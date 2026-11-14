using Reference.Application.Features.Fabric.Queries.GetAllFabric;

namespace Reference.Api.Endpoints.Fabrics;

internal static class GetFabricsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).Produces<List<FabricRowDTO>>()
            .RequireAuthorization("CatalogRead");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetAllFabricQuery(), cancellationToken)).ToHttpResult();
}
