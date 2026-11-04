using Production.Application.Contracts;
using Production.Application.Orders.Access;

namespace Production.Application.Orders.CreateOrder;

/// <summary>«Передати в цех»: an order from the person's sent КП.</summary>
public sealed record CreateProductionOrderCommand(ProductionActor Actor, CreateProductionOrderInput Input)
    : ICommand<Result<ProductionOrderResponse>>;
