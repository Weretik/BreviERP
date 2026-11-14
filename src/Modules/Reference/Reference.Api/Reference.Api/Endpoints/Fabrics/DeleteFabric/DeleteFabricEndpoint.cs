using Reference.Application.Features.Fabric.Commands.DeleteFabric;

namespace Reference.Api.Endpoints.Fabrics.DeleteFabric;

internal static class DeleteFabricEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("{id:int}", HandleAsync)
            .RequireAuthorization("access:materials");

    private static async Task<IResult> HandleAsync(int id, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new DeleteFabricCommand(id), cancellationToken)).ToHttpResult();
}
