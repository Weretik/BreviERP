using Production.Application.Features.Orders.Commands.CopyProductionOrder;

namespace Production.Api.Endpoints.Orders;

internal static class CopyProductionOrderEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{id:int}/copy", HandleAsync).WithName("copyProductionOrder");

    private static async Task<IResult> HandleAsync(int id, CopyProductionOrderInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        user.Actor() is { } actor
            ? (await sender.Send(new CopyProductionOrderCommand(actor, id, input), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
