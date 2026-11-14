using Catalog.Domain.Pricing.Sewing;

namespace Catalog.Application.Features.Pricing.Queries.GetSewingArticleSummary;

/// <summary>
/// Everything the sewing card shows in one response (CAT-007, CAT-016): every fabric variant with its
/// live price, and the shared composition with reference names and prices.
/// </summary>
public sealed record SewingArticleSummary(
    int ProductId, string? ModelCode, long Version, DateTimeOffset CalculatedAt,
    IReadOnlyList<SewingArticleSummaryRow> Articles,
    IReadOnlyList<SewingCardMaterial> Materials,
    IReadOnlyList<SewingCardOperation> Operations);

public sealed record SewingArticleSummaryRow(
    long Id, string SaleSku, bool Active, int SortOrder,
    int? FabricId, string? FabricName, decimal? FabricPrice, decimal? FabricQuantity,
    bool IsCalculated,
    IReadOnlyList<SewingPricingBlocker> Blockers, SewingCostBreakdown? Costs, SewingPriceTiers? Prices,
    SewingArticlePublishedSummary? Published);

public sealed record SewingArticlePublishedSummary(DateTimeOffset PublishedAt, SewingPriceTiers Prices);

/// <summary>A shared composition line; name and unit price come from the current reference (null when unknown).</summary>
public sealed record SewingCardMaterial(
    long Id, string Kind, int? ReferenceId, string? CostCode, string? Name, decimal? UnitPrice,
    decimal Quantity, string Unit, int SortOrder, string? Comment);

/// <summary>An operation of the model; norm minutes come from the reference «Операції» (null when unknown).</summary>
public sealed record SewingCardOperation(
    long Id, int OperationId, string? Name, decimal? NormMinutes,
    decimal Quantity, decimal? ManualMinutes, int SortOrder, string? Comment);
