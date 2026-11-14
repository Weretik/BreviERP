namespace Warehouse.Application.Features.Stock.Shared;

/// <summary>Plan and fact of one planned part in an issue by cutting (TZ §7.2).</summary>
/// <param name="Plan">P × n / N, half up to hundredths; counted once for every line of the part.</param>
/// <param name="Difference">Fact minus plan; positive is overspending.</param>
/// <param name="PerProduct">Fact per product cut.</param>
public sealed record FabricIssueComparisonResponse(
    string PlanFabricCode,
    string PlanColorKey,
    string Color,
    decimal Plan,
    decimal Actual,
    decimal Difference,
    decimal PerProduct);
