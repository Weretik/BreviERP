using Production.Domain.Orders;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Responses;

namespace Production.Application.Features.Orders.Commands.CancelProductionShipment;

/// <summary>Removes a wrongly recorded shipment; the history keeps it.</summary>
public sealed record CancelProductionShipmentCommand(ProductionActor Actor, int OrderId, int ShipmentId, int Version)
    : ICommand<Result<ProductionOrderResponse>>;
