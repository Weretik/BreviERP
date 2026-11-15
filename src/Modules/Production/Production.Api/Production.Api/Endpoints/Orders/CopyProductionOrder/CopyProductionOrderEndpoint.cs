using Production.Application.Features.Orders.Shared.Responses;
using Production.Application.Features.Orders.Commands.CopyProductionOrder;
using BuildingBlocks.Api.Errors;

namespace Production.Api.Endpoints.Orders.CopyProductionOrder;

internal static class CopyProductionOrderEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{id:int}/copy", HandleAsync).WithName("copyProductionOrder")
            .WithSummary("«Копіювати»: a new «Нове» order of the person who copies, dated today")
            .Produces<ProductionOrderResponse>();

    private static async Task<IResult> HandleAsync(int id, CopyProductionOrderInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        user.Actor() is { } actor
            ? (await sender.Send(new CopyProductionOrderCommand(actor, id, input), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
