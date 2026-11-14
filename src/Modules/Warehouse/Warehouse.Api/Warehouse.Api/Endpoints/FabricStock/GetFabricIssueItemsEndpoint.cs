using Warehouse.Application.Features.Stock.Queries.GetFabricIssueItems;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Api.Endpoints.FabricStock;

/// <summary>«Видача на завдання»: open items found by order number, client, model or fabric; newest first.</summary>
internal static class GetFabricIssueItemsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("issue-items", HandleAsync).WithName("getFabricIssueItems").Produces<FabricIssueItemsResponse>();

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, [FromServices] ISender sender, string? search = null, CancellationToken cancellationToken = default) =>
        user.WarehouseActor() is { } actor
            ? (await sender.Send(new GetFabricIssueItemsQuery(actor, search), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
