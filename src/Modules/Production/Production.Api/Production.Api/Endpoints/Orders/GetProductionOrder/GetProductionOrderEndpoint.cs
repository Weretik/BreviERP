using Production.Application.Features.Orders.Shared.Responses;
using Production.Application.Features.Orders.Queries.GetProductionOrder;
using BuildingBlocks.Api.Errors;

namespace Production.Api.Endpoints.Orders.GetProductionOrder;

internal static class GetProductionOrderEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("{id:int}", HandleAsync).WithName("getProductionOrder")
            .WithSummary("An order with its items; prices are hidden by role")
            .Produces<ProductionOrderResponse>();

    private static async Task<IResult> HandleAsync(int id, ClaimsPrincipal user, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        user.Actor() is { } actor
            ? (await sender.Send(new GetProductionOrderQuery(actor, id), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
