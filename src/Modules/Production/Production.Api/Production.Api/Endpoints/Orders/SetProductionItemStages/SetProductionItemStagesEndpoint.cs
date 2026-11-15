using Production.Application.Features.Orders.Shared.Responses;
using Production.Application.Features.Orders.Commands.SetProductionItemStages;
using BuildingBlocks.Api.Errors;

namespace Production.Api.Endpoints.Orders.SetProductionItemStages;

/// <summary>Ticks the stages of an item; the furthest one colours the row.</summary>
internal static class SetProductionItemStagesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{id:int}/items/{itemId:int}/stages", HandleAsync).WithName("setProductionItemStages")
            .WithSummary("Tick the stages of an item; the furthest one colours the row")
            .Produces<ProductionOrderResponse>();

    private static async Task<IResult> HandleAsync(int id, int itemId, SetProductionItemStagesInput input,
        ClaimsPrincipal user, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        user.Actor() is { } actor
            ? (await sender.Send(new SetProductionItemStagesCommand(actor, id, itemId, input),
                cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
