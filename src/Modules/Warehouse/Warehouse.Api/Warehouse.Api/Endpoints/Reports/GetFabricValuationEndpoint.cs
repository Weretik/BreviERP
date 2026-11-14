using Warehouse.Application.Features.Stock.Queries.GetFabricValuation;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Api.Endpoints.Reports;

/// <summary>«Вартість складу»: positive balances at today's prices of «Тканини»; shortages apart.</summary>
internal static class GetFabricValuationEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("valuation", HandleAsync).WithName("getFabricValuation").Produces<FabricValuationResponse>()
            .RequireAuthorization(WarehouseAccess.FabricValuation);

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        user.WarehouseActor() is { } actor
            ? (await sender.Send(new GetFabricValuationQuery(actor), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
