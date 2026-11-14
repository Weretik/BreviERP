namespace Warehouse.Application.Features.Stock.Commands.CreateFabricIssue;

/// <summary>A line of an issue by cutting: real metres of one pair of the stock.</summary>
/// <param name="FabricCode">Any code of the fabric's alias group, as «Залишки» show it.</param>
/// <param name="ExpectedVersion">The pair version the form showed; null skips the check.</param>
/// <param name="PlanFabricCode">With <paramref name="PlanColorKey"/>: the planned part this line is for; null for additional.</param>
/// <param name="Layer">Role of an additional line: Upper, Lining, Insulation or Fleece; null when unknown.</param>
public sealed record FabricIssueLineInput(
    string FabricCode,
    string Color,
    string? ExpectedVersion,
    decimal Meters,
    string? PlanFabricCode,
    string? PlanColorKey,
    string? Layer,
    string? Note);
