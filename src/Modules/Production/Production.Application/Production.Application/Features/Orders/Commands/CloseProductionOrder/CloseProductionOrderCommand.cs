using Production.Domain.Orders;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Access;

namespace Production.Application.Features.Orders.Commands.CloseProductionOrder;

/// <summary>«Закрити»: everything is shipped.</summary>
public sealed record CloseProductionOrderCommand(ProductionActor Actor, int OrderId, int Version)
    : ICommand<Result<ProductionOrderResponse>>;
