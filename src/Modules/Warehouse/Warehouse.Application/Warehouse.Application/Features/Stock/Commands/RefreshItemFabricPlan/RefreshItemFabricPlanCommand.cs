using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Commands.CreateFabricIssue;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Commands.RefreshItemFabricPlan;

/// <summary>
/// «Оновити план»: the norm of today replaces the saved plan of an item (TZ §5.2). Issues already made keep the plan
/// they were compared with; the change is written to the item history.
/// </summary>
public sealed record RefreshItemFabricPlanCommand(WarehouseActor Actor, int OrderId, int ItemId,
    RefreshItemFabricPlanInput Input) : ICommand<Result<ProductionItemFabricResponse>>;
