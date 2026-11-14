using Production.Application.Abstractions;
using Production.Domain.Work;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Work.Shared;

namespace Production.Application.Features.Work.Commands.CreateProductionWork;

/// <summary>
/// «Додати роботу»: a worker sewed or cut pieces of an item (PRD-D34, D35). Within the plan the item's quantity is the
/// limit per kind; more goes as «понад план» with a reason.
/// </summary>
public sealed record CreateProductionWorkCommand(ProductionActor Actor, int OrderId, int ItemId,
    CreateProductionWorkInput Input) : ICommand<Result<ProductionItemWorkResponse>>;
