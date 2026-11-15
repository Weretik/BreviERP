using Reference.Application.Features.GarmentAccessory.Commands.DeleteGarmentAccessory;
using BuildingBlocks.Api.Errors;

namespace Reference.Api.Endpoints.GarmentAccessories.DeleteGarmentAccessory;

internal static class DeleteGarmentAccessoryEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("{id:int}", HandleAsync)
            .WithName("deleteGarmentAccessory")
            .WithSummary("Delete a garment accessory")
            .WithDescription("Requires access:materials. Numeric edits append an approved pricing revision when the reference is versioned.")
            .RequireAuthorization("access:materials");

    private static async Task<IResult> HandleAsync(int id, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new DeleteGarmentAccessoryCommand(id), cancellationToken)).ToHttpResult();
}
