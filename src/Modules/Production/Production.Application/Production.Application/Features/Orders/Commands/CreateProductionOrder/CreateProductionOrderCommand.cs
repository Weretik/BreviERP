using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Responses;

namespace Production.Application.Features.Orders.Commands.CreateProductionOrder;

/// <summary>«Передати в цех»: an order from the person's КП «під запит».</summary>
public sealed record CreateProductionOrderCommand(ProductionActor Actor, CreateProductionOrderInput Input)
    : ICommand<Result<ProductionOrderResponse>>;
