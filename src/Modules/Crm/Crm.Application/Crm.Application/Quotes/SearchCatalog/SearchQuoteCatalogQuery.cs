using Crm.Application.Quotes.Contracts;

namespace Crm.Application.Quotes.SearchCatalog;

/// <summary>Products the «Додати товар» panel offers: sewing models with fabric variants or PPE.</summary>
/// <param name="Kind">`Sewing` or `Ppe`.</param>
/// <param name="Season">`Summer` or `Winter`: only sewing models of that season (the introduction pages).</param>
public sealed record SearchQuoteCatalogQuery(string Kind, string? Search, int Page = 1, int PageSize = 20,
    string? Season = null)
    : IQuery<Result<QuoteCatalogPageResponse>>;
