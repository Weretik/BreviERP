using Production.Application.Features.FabricNorms.Queries.GetFabricNorms;
using BuildingBlocks.Api.Errors;

namespace Production.Api.Endpoints.FabricNorms.GetFabricNorms;

/// <summary>Every Catalog sewing model with its norm (or none), then norms whose model is gone.</summary>
internal static class GetFabricNormsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getFabricNorms");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetFabricNormsQuery(), cancellationToken)).ToHttpResult();
}
