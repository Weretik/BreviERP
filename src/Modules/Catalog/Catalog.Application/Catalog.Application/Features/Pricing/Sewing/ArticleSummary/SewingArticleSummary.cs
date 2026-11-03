using Catalog.Domain.Pricing.Sewing;

namespace Catalog.Application.Features.Pricing.Sewing.ArticleSummary;

public sealed record SewingArticleSummary(
    int ProductId, string? ModelCode, long Version, DateTimeOffset CalculatedAt,
    IReadOnlyList<SewingArticleSummaryRow> Articles);

public sealed record SewingArticleSummaryRow(
    long Id, string SaleSku, bool Active, int SortOrder, bool IsCalculated,
    IReadOnlyList<SewingPricingBlocker> Blockers, SewingCostBreakdown? Costs, SewingPriceTiers? Prices,
    SewingArticlePublishedSummary? Published);

public sealed record SewingArticlePublishedSummary(DateTimeOffset PublishedAt, SewingPriceTiers Prices);
