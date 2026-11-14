using Production.Application.Features.FabricNorms.Commands.DeleteFabricNorm;

namespace Production.Api.Endpoints.FabricNorms;

internal static class DeleteFabricNormEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("{modelCode}", HandleAsync).WithName("deleteFabricNorm");

    private static async Task<IResult> HandleAsync(string modelCode, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new DeleteFabricNormCommand(modelCode), cancellationToken)).ToNoContentHttpResult();
}
