using Warehouse.Application.Features.Stock.Queries.GetHardwareOrder;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Api.Endpoints.Reports.GetHardwareOrder;

/// <summary>«Фурнітура до замовлення»: the full accessory need of open orders by model composition.</summary>
internal static class GetHardwareOrderEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("hardware", HandleAsync).WithName("getHardwareOrder").Produces<HardwareOrderResponse>()
            .RequireAuthorization(WarehouseAccess.HardwareOrder);

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        user.WarehouseActor() is { } actor
            ? (await sender.Send(new GetHardwareOrderQuery(actor), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
