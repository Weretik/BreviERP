using Production.Application.Features.Orders.Queries.GetProductionEmbroideries;

namespace Production.Api.Endpoints.Orders.GetProductionEmbroideries;

/// <summary>«Розрахунок вишивки»: saved «Ціна вишивки» calculations to link to an embroidery.</summary>
internal static class GetProductionEmbroideriesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("embroideries", HandleAsync).WithName("getProductionEmbroideries");

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, [FromServices] ISender sender,
        string? search = null, CancellationToken cancellationToken = default) =>
        user.Actor() is { } actor
            ? (await sender.Send(new GetProductionEmbroideriesQuery(actor, search), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
