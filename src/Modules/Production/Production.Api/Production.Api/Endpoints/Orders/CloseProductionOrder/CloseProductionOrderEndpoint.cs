using Production.Application.Features.Orders.Commands.CloseProductionOrder;
using Production.Application.Features.Orders.Shared;

namespace Production.Api.Endpoints.Orders.CloseProductionOrder;

/// <summary>«Закрити»: everything is shipped.</summary>
internal static class CloseProductionOrderEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{id:int}/close", HandleAsync).WithName("closeProductionOrder")
            .RequireAccessAction(AccessActionAttribute.Edit);

    private static async Task<IResult> HandleAsync(int id, ProductionOrderVersionInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        user.Actor() is { } actor
            ? (await sender.Send(new CloseProductionOrderCommand(actor, id, input.Version),
                cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
