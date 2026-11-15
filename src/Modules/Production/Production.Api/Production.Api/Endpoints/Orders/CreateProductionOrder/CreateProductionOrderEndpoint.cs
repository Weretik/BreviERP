using Production.Application.Features.Orders.Shared.Responses;
using Production.Application.Features.Orders.Commands.CreateProductionOrder;
using BuildingBlocks.Api.Errors;

namespace Production.Api.Endpoints.Orders.CreateProductionOrder;

/// <summary>«Передати в цех». A repeat with the same request id returns the same order.</summary>
internal static class CreateProductionOrderEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("", HandleAsync).WithName("createProductionOrder")
            .WithSummary("«Передати в цех»: an order from a КП «під запит» (saved or sent); a repeat with the same requestId returns the same order")
            .Produces<ProductionOrderResponse>();

    private static async Task<IResult> HandleAsync(CreateProductionOrderInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        user.Actor() is { } actor
            ? (await sender.Send(new CreateProductionOrderCommand(actor, input), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
