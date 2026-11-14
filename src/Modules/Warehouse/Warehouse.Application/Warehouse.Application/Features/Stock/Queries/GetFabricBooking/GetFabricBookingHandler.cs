using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;
using Warehouse.Application.Features.Stock.Shared.Booking;

namespace Warehouse.Application.Features.Stock.Queries.GetFabricBooking;

public sealed class GetFabricBookingHandler(FabricBookingCalculator booking)
    : IQueryHandler<GetFabricBookingQuery, Result<FabricBookingResponse>>
{
    public async ValueTask<Result<FabricBookingResponse>> Handle(GetFabricBookingQuery query,
        CancellationToken cancellationToken) =>
        Result.Success(await booking.CalculateAsync(cancellationToken) with { CanReceive = query.Actor.CanMoveStock });
}
