using Production.Application.Features.Orders.Commands.CancelProductionOrder;

namespace Production.Api.Endpoints.Orders.CancelProductionOrder;

/// <summary>«Скасувати» with a reason.</summary>
internal static class CancelProductionOrderEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{id:int}/cancel", HandleAsync).WithName("cancelProductionOrder")
            .RequireAccessAction(AccessActionAttribute.Edit);

    private static async Task<IResult> HandleAsync(int id, CancelProductionOrderInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        user.Actor() is { } actor
            ? (await sender.Send(new CancelProductionOrderCommand(actor, id, input), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
