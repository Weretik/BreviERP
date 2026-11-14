using Production.Application.Features.Orders.Commands.DeleteProductionOrderFile;
using BuildingBlocks.Api.Errors;

namespace Production.Api.Endpoints.Orders.DeleteProductionOrderFile;

internal static class DeleteProductionOrderFileEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("{id:int}/files/{fileId:int}", HandleAsync).WithName("deleteProductionOrderFile");

    private static async Task<IResult> HandleAsync(int id, int fileId, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        user.Actor() is { } actor
            ? (await sender.Send(new DeleteProductionOrderFileCommand(actor, id, fileId),
                cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
