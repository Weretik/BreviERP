using Production.Domain.Orders;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Responses;

namespace Production.Application.Features.Orders.Commands.CancelProductionOrder;

/// <summary>«Скасувати» with a reason.</summary>
public sealed record CancelProductionOrderCommand(ProductionActor Actor, int OrderId, CancelProductionOrderInput Input)
    : ICommand<Result<ProductionOrderResponse>>;
