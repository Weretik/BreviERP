using Production.Domain.Orders;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Validation;

namespace Production.Application.Features.Orders.Commands.SetProductionItemStages;

/// <summary>Ticks the етапи of an item; several may be ticked at once, as in the old «Цех».</summary>
public sealed record SetProductionItemStagesCommand(
    ProductionActor Actor, int OrderId, int ItemId, SetProductionItemStagesInput Input)
    : ICommand<Result<ProductionOrderResponse>>;
