using Production.Application.Features.FabricNorms.Commands.DeleteFabricNorm;
using BuildingBlocks.Api.Errors;

namespace Production.Api.Endpoints.FabricNorms.DeleteFabricNorm;

internal static class DeleteFabricNormEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("{modelCode}", HandleAsync).WithName("deleteFabricNorm")
            .WithSummary("Delete the norm");

    private static async Task<IResult> HandleAsync(string modelCode, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new DeleteFabricNormCommand(modelCode), cancellationToken)).ToNoContentHttpResult();
}
