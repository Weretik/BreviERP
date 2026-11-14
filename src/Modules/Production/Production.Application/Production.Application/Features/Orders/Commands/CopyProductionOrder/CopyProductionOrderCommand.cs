using Production.Application.Abstractions;
using Production.Domain.Orders;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Validation;

namespace Production.Application.Features.Orders.Commands.CopyProductionOrder;

/// <summary>
/// «Копіювати»: a new order with the same client, КП and items (with their sewing rate and КП price). It starts as
/// «Нове» today; the person who copies is the manager. A repeated request returns the same copy.
/// </summary>
public sealed record CopyProductionOrderCommand(ProductionActor Actor, int Id, CopyProductionOrderInput Input)
    : ICommand<Result<ProductionOrderResponse>>;
