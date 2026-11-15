using Production.Application.Features.Orders.Shared.Responses;
using Production.Application.Features.Orders.Commands.CancelProductionShipment;
using BuildingBlocks.Api.Errors;

namespace Production.Api.Endpoints.Orders.CancelProductionShipment;

internal static class CancelProductionShipmentEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("{id:int}/shipments/{shipmentId:int}", HandleAsync).WithName("cancelProductionShipment")
            .WithSummary("Cancel a shipment recorded by mistake")
            .Produces<ProductionOrderResponse>();

    private static async Task<IResult> HandleAsync(int id, int shipmentId, ClaimsPrincipal user,
        [FromServices] ISender sender, int version = default, CancellationToken cancellationToken = default) =>
        user.Actor() is { } actor
            ? (await sender.Send(new CancelProductionShipmentCommand(actor, id, shipmentId, version),
                cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
