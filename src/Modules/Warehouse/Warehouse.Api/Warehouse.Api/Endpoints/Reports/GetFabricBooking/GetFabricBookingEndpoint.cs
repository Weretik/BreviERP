using Warehouse.Application.Features.Stock.Queries.GetFabricBooking;
using Warehouse.Application.Features.Stock.Shared;
using Warehouse.Application.Features.Stock.Shared.Booking;
using BuildingBlocks.Api.Errors;

namespace Warehouse.Api.Endpoints.Reports.GetFabricBooking;

/// <summary>«Бронь і що замовити»: the stock handed to the plans of open orders in order; what to order.</summary>
internal static class GetFabricBookingEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("booking", HandleAsync).WithName("getFabricBooking")
            .WithSummary("«Бронь і що замовити»: stock handed to the plans of open orders by order date; reads only (access:fabric-booking)").Produces<FabricBookingResponse>()
            .RequireAuthorization(WarehouseAccess.FabricBooking);

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        user.WarehouseActor() is { } actor
            ? (await sender.Send(new GetFabricBookingQuery(actor), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
