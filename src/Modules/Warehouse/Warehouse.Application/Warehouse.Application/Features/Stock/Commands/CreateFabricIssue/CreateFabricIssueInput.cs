namespace Warehouse.Application.Features.Stock.Commands.CreateFabricIssue;

/// <summary>«Видача» by actual cutting: products cut now and the real fabric (TZ §7).</summary>
/// <param name="PlanVersion">The plan version the form showed (0 before the first issue).</param>
public sealed record CreateFabricIssueInput(
    Guid OperationId,
    int OrderVersion,
    int PlanVersion,
    DateOnly IssuedOn,
    int IssuedQuantity,
    string? Note,
    IReadOnlyList<FabricIssueLineInput> Lines);
