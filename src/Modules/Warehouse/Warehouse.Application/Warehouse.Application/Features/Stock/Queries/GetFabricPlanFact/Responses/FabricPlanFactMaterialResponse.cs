namespace Warehouse.Application.Features.Stock.Queries.GetFabricPlanFact.Responses;

/// <summary>A planned part, or a material issued beyond the plan.</summary>
/// <param name="Layer">Upper, Lining, Insulation, Fleece, or null when the role is unknown.</param>
/// <param name="Fact">Issued metres; null when nothing was issued for a planned part («не зафіксовано»).</param>
/// <param name="Used">The fabrics and colours actually issued.</param>
public sealed record FabricPlanFactMaterialResponse(
    string? Layer,
    string? PlanFabricCode,
    string? PlanFabricName,
    string? PlanColor,
    decimal? Plan,
    decimal? Fact,
    decimal? Difference,
    IReadOnlyList<string> Used);
