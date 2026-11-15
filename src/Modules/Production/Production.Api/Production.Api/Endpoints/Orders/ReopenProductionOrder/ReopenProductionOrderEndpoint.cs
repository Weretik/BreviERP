using Production.Application.Features.Orders.Shared.Responses;
using Production.Application.Features.Orders.Commands.ReopenProductionOrder;
using Production.Application.Features.Orders.Shared;
using BuildingBlocks.Api.Errors;

namespace Production.Api.Endpoints.Orders.ReopenProductionOrder;

/// <summary>«Повернути в роботу» (Admin).</summary>
internal static class ReopenProductionOrderEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{id:int}/reopen", HandleAsync).WithName("reopenProductionOrder")
            .WithSummary("Admin: «Повернути в роботу» a closed or cancelled order")
            .RequireAccessAction(AccessActionAttribute.Edit)
            .Produces<ProductionOrderResponse>();

    private static async Task<IResult> HandleAsync(int id, ProductionOrderVersionInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        user.Actor() is { } actor
            ? (await sender.Send(new ReopenProductionOrderCommand(actor, id, input.Version),
                cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
