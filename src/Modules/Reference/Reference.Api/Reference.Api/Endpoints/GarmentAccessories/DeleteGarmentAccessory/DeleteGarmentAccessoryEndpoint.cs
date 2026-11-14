using Reference.Application.Features.GarmentAccessory.Commands.DeleteGarmentAccessory;
using BuildingBlocks.Api.Errors;

namespace Reference.Api.Endpoints.GarmentAccessories.DeleteGarmentAccessory;

internal static class DeleteGarmentAccessoryEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("{id:int}", HandleAsync)
            .RequireAuthorization("access:materials");

    private static async Task<IResult> HandleAsync(int id, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new DeleteGarmentAccessoryCommand(id), cancellationToken)).ToHttpResult();
}
