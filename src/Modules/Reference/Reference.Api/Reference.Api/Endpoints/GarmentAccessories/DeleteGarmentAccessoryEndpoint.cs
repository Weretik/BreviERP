using Reference.Application.Features.GarmentAccessory.Commands.DeleteGarmentAccessory;

namespace Reference.Api.Endpoints.GarmentAccessories;

internal static class DeleteGarmentAccessoryEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("{id:int}", HandleAsync)
            .RequireAuthorization("access:materials");

    private static async Task<IResult> HandleAsync(int id, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new DeleteGarmentAccessoryCommand(id), cancellationToken)).ToHttpResult();
}
