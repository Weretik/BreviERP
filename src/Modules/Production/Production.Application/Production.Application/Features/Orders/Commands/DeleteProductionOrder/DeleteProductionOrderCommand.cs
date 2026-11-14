using Production.Application.Abstractions;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Access;

namespace Production.Application.Features.Orders.Commands.DeleteProductionOrder;

/// <summary>«Видалити» (Admin only): the order leaves the lists; its rows and history stay.</summary>
public sealed record DeleteProductionOrderCommand(ProductionActor Actor, int Id, int Version) : ICommand<Result>;
