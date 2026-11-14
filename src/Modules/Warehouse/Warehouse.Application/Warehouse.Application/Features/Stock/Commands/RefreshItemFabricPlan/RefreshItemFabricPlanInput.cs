namespace Warehouse.Application.Features.Stock.Commands.RefreshItemFabricPlan;

/// <summary>«Оновити план»: the norm of today replaces the saved plan of the item.</summary>
public sealed record RefreshItemFabricPlanInput(int OrderVersion, int PlanVersion);
