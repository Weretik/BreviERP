using Reference.Application.Features.GarmentPartOperation.Commands.DeleteGarmentPartOperation;
using BuildingBlocks.Api.Errors;

namespace Reference.Api.Endpoints.GarmentPartOperations.DeleteGarmentPartOperation;

internal static class DeleteGarmentPartOperationEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("{id:int}", HandleAsync)
            .WithName("deleteGarmentPartOperation")
            .WithSummary("Delete a garment part operation")
            .WithDescription("Requires access:operations. Numeric edits append an approved pricing revision when the reference is versioned.")
            .RequireAuthorization("access:operations");

    private static async Task<IResult> HandleAsync(int id, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new DeleteGarmentPartOperationCommand(id), cancellationToken)).ToHttpResult();
}
