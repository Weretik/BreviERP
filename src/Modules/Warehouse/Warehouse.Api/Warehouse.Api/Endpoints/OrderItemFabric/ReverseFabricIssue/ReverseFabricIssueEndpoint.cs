using Warehouse.Application.Features.Stock.Commands.ReverseFabricIssue;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Api.Endpoints.OrderItemFabric.ReverseFabricIssue;

/// <summary>«Сторно видачі».</summary>
internal static class ReverseFabricIssueEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{id:int}/fabric-issues/{issueId:int}/reverse", HandleAsync).WithName("reverseFabricIssue").Produces<ProductionItemFabricResponse>().RequireAccessAction(AccessActionAttribute.Edit);

    private static async Task<IResult> HandleAsync(int id, int issueId, ReverseFabricIssueInput input, ClaimsPrincipal user, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        user.WarehouseActor() is { } actor
            ? (await sender.Send(new ReverseFabricIssueCommand(actor, id, issueId, input), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
