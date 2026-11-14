namespace Catalog.Application.Features.Pricing.Queries.GetSewingArticleSummary;

/// <summary>Live, non-saving calculation of every Article of one Sewing model for the product card (CAT-007).</summary>
public sealed record GetSewingArticleSummaryQuery(int ProductId) : IQuery<Result<SewingArticleSummary>>;
