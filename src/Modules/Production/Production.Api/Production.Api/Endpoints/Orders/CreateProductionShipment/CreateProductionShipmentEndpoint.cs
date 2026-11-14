using Production.Application.Features.Orders.Commands.CreateProductionShipment;

namespace Production.Api.Endpoints.Orders.CreateProductionShipment;

/// <summary>«Відвантажити» part of an item.</summary>
internal static class CreateProductionShipmentEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{id:int}/items/{itemId:int}/shipments", HandleAsync).WithName("createProductionShipment");

    private static async Task<IResult> HandleAsync(int id, int itemId, CreateProductionShipmentInput input,
        ClaimsPrincipal user, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        user.Actor() is { } actor
            ? (await sender.Send(new CreateProductionShipmentCommand(actor, id, itemId, input),
                cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
