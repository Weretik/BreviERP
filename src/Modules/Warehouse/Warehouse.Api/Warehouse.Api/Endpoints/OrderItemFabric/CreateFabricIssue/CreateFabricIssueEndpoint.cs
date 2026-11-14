using Warehouse.Application.Features.Stock.Commands.CreateFabricIssue;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Api.Endpoints.OrderItemFabric.CreateFabricIssue;

/// <summary>«Видати» from the item card.</summary>
internal static class CreateFabricIssueEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{id:int}/items/{itemId:int}/fabric-issues", HandleAsync).WithName("createFabricIssue").Produces<ProductionItemFabricResponse>();

    private static async Task<IResult> HandleAsync(int id, int itemId, CreateFabricIssueInput input, ClaimsPrincipal user, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        user.WarehouseActor() is { } actor
            ? (await sender.Send(new CreateFabricIssueCommand(actor, id, itemId, input), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
