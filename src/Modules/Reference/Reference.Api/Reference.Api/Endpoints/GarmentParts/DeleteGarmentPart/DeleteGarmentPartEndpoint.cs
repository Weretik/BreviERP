using Reference.Application.Features.GarmentPart.Commands.DeleteGarmentPart;

namespace Reference.Api.Endpoints.GarmentParts.DeleteGarmentPart;

internal static class DeleteGarmentPartEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("{id:int}", HandleAsync);

    private static async Task<IResult> HandleAsync(int id, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new DeleteGarmentPartCommand(id), cancellationToken)).ToHttpResult();
}
