using Reference.Application.Features.Fabric.Queries.GetAllFabric;
using BuildingBlocks.Api.Errors;

namespace Reference.Api.Endpoints.Fabrics.GetFabrics;

internal static class GetFabricsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).Produces<List<FabricRowDTO>>()
            .RequireAuthorization("CatalogRead");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetAllFabricQuery(), cancellationToken)).ToHttpResult();
}
