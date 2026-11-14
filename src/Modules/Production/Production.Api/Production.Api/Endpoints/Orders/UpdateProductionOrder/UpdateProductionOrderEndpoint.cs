using Production.Application.Features.Orders.Commands.UpdateProductionOrder;
using BuildingBlocks.Api.Errors;

namespace Production.Api.Endpoints.Orders.UpdateProductionOrder;

internal static class UpdateProductionOrderEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{id:int}", HandleAsync).WithName("updateProductionOrder");

    private static async Task<IResult> HandleAsync(int id, UpdateProductionOrderInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        user.Actor() is { } actor
            ? (await sender.Send(new UpdateProductionOrderCommand(actor, id, input), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
