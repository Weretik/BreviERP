using Production.Application.Features.Orders.Commands.DeleteProductionOrder;
using BuildingBlocks.Api.Errors;

namespace Production.Api.Endpoints.Orders.DeleteProductionOrder;

internal static class DeleteProductionOrderEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("{id:int}", HandleAsync).WithName("deleteProductionOrder");

    private static async Task<IResult> HandleAsync(int id, ClaimsPrincipal user, [FromServices] ISender sender,
        int version = default, CancellationToken cancellationToken = default) =>
        user.Actor() is { } actor
            ? (await sender.Send(new DeleteProductionOrderCommand(actor, id, version),
                cancellationToken)).ToNoContentHttpResult()
            : Results.Forbid();
}
