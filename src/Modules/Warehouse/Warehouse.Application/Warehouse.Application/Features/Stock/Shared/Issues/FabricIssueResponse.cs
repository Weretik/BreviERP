namespace Warehouse.Application.Features.Stock.Shared.Issues;

/// <param name="IssuedQuantity">Products cut; null for issues before issues by cutting.</param>
/// <param name="Comparison">Plan and fact by planned part; empty for older issues.</param>
public sealed record FabricIssueResponse(
    int Id,
    DateOnly IssuedOn,
    string? Note,
    string CreatedByName,
    DateTimeOffset CreatedAt,
    bool IsReversed,
    DateTimeOffset? ReversedAt,
    string? ReversedByName,
    string? ReverseReason,
    int? IssuedQuantity,
    IReadOnlyList<FabricIssueComparisonResponse> Comparison,
    IReadOnlyList<FabricIssueLineResponse> Lines);
