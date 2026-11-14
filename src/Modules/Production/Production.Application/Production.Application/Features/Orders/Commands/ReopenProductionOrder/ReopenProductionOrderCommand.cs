using Production.Domain.Orders;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Responses;

namespace Production.Application.Features.Orders.Commands.ReopenProductionOrder;

/// <summary>«Повернути в роботу» (Admin).</summary>
public sealed record ReopenProductionOrderCommand(ProductionActor Actor, int OrderId, int Version)
    : ICommand<Result<ProductionOrderResponse>>;
