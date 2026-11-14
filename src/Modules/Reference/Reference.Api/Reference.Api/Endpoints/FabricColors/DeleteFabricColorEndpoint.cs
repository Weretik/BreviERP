using Reference.Application.Features.FabricColor.Commands.DeleteFabricColor;

namespace Reference.Api.Endpoints.FabricColors;

internal static class DeleteFabricColorEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("{id:int}", HandleAsync).WithName("deleteFabricColor")
            .RequireAuthorization("access:materials");

    private static async Task<IResult> HandleAsync(int id, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new DeleteFabricColorCommand(id), cancellationToken)).ToHttpResult();
}
