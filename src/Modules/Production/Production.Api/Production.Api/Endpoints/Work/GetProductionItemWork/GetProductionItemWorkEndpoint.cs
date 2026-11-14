using Production.Application.Features.Work.Queries.GetProductionItemWork;
using BuildingBlocks.Api.Errors;

namespace Production.Api.Endpoints.Work.GetProductionItemWork;

internal static class GetProductionItemWorkEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("orders/{id:int}/items/{itemId:int}/work", HandleAsync).WithName("getProductionItemWork");

    private static async Task<IResult> HandleAsync(int id, int itemId, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        user.Actor() is { } actor
            ? (await sender.Send(new GetProductionItemWorkQuery(actor, id, itemId), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
