using Reference.Application.Features.GarmentPartOperation.Commands.DeleteGarmentPartOperation;
using BuildingBlocks.Api.Errors;

namespace Reference.Api.Endpoints.GarmentPartOperations.DeleteGarmentPartOperation;

internal static class DeleteGarmentPartOperationEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("{id:int}", HandleAsync)
            .RequireAuthorization("access:operations");

    private static async Task<IResult> HandleAsync(int id, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new DeleteGarmentPartOperationCommand(id), cancellationToken)).ToHttpResult();
}
