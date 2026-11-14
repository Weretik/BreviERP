using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;
using Warehouse.Application.Features.Stock.Shared.Booking;

namespace Warehouse.Application.Features.Stock.Queries.GetFabricBooking;

/// <summary>
/// «Бронь і що замовити» (TZ §12): for every fabric and colour, the stock is handed to the plans of open orders in the
/// order the orders were created; what is left over is free, what is missing is to order. Reads only.
/// </summary>
public sealed record GetFabricBookingQuery(WarehouseActor Actor) : IQuery<Result<FabricBookingResponse>>;
