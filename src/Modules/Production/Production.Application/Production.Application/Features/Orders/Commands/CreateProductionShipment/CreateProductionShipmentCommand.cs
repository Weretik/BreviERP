using Production.Domain.Orders;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Responses;

namespace Production.Application.Features.Orders.Commands.CreateProductionShipment;

/// <summary>«Відвантажити»: part of an item goes to the client; more than is left is refused.</summary>
public sealed record CreateProductionShipmentCommand(
    ProductionActor Actor, int OrderId, int ItemId, CreateProductionShipmentInput Input)
    : ICommand<Result<ProductionOrderResponse>>;
