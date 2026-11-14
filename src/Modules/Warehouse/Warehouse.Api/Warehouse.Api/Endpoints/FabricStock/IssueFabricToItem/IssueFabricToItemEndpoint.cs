using Warehouse.Application.Features.Stock.Commands.CreateFabricIssue;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Api.Endpoints.FabricStock.IssueFabricToItem;

/// <summary>«Видача» by actual cutting: products cut now and the real pairs; one package per operation id.</summary>
internal static class IssueFabricToItemEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("issue-items/{orderId:int}/{itemId:int}/issues", HandleAsync).WithName("issueFabricToItem").Produces<ProductionItemFabricResponse>();

    private static async Task<IResult> HandleAsync(int orderId, int itemId, CreateFabricIssueInput input, ClaimsPrincipal user, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        user.WarehouseActor() is { } actor
            ? (await sender.Send(new CreateFabricIssueCommand(actor, orderId, itemId, input), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
