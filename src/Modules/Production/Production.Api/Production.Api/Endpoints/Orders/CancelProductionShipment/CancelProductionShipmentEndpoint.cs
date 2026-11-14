using Production.Application.Features.Orders.Commands.CancelProductionShipment;

namespace Production.Api.Endpoints.Orders.CancelProductionShipment;

internal static class CancelProductionShipmentEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("{id:int}/shipments/{shipmentId:int}", HandleAsync).WithName("cancelProductionShipment");

    private static async Task<IResult> HandleAsync(int id, int shipmentId, ClaimsPrincipal user,
        [FromServices] ISender sender, int version = default, CancellationToken cancellationToken = default) =>
        user.Actor() is { } actor
            ? (await sender.Send(new CancelProductionShipmentCommand(actor, id, shipmentId, version),
                cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
