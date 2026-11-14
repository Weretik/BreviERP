namespace Warehouse.Application.Features.Stock.Queries.GetFabricPlanFact;

// ── «План і факт» (TZ §11) ─────────────────────────────────────────────────────────────────────────────────────────

/// <summary>
/// «План і факт основної тканини за моделями»: items whose fabric was issued for all their products, by model, and the
/// total. <see cref="FabricPlanFactRowResponse.Result"/> is Economy, Overrun, OnPlan, NoPlan or NeedsCheck.
/// </summary>
/// <param name="InProgressItems">Items with issues that did not cover all their products yet; left out.</param>
public sealed record FabricPlanFactResponse(
    IReadOnlyList<FabricPlanFactModelResponse> Models,
    FabricPlanFactRowResponse Total,
    int InProgressItems);
