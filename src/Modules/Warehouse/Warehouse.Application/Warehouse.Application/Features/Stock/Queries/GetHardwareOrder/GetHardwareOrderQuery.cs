using System.Text;
using Warehouse.Application.Abstractions;
using Warehouse.Application.Features.Stock.Queries.GetFabricBooking;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Queries.GetHardwareOrder;

/// <summary>
/// «Фурнітура до замовлення» (TZ §14): the full need of physical accessories of every open order (any item stage), by
/// the composition of the models. Stock, purchases and issues are not taken off.
/// </summary>
public sealed record GetHardwareOrderQuery(WarehouseActor Actor) : IQuery<Result<HardwareOrderResponse>>;
