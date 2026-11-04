using Crm.Application.Quotes.Catalog;
using Crm.Application.Quotes.Contracts;
using Crm.Application.Quotes.Introductions.Building;
using Crm.Application.Quotes.Mapping;
using Crm.Domain.Quotes;

namespace Crm.Application.Quotes.SearchCatalog;

public sealed class SearchQuoteCatalogQueryHandler(IQuoteCatalogReader catalog)
    : IQueryHandler<SearchQuoteCatalogQuery, Result<QuoteCatalogPageResponse>>
{
    public async ValueTask<Result<QuoteCatalogPageResponse>> Handle(SearchQuoteCatalogQuery query,
        CancellationToken cancellationToken)
    {
        var season = query.Season is null
            ? null
            : QuoteIntroPages.CatalogSeason(Enum.Parse<QuoteIntroSeason>(query.Season));
        var page = await catalog.SearchAsync(Enum.Parse<QuoteItemKind>(query.Kind), query.Search?.Trim(), season,
            query.Page, query.PageSize, cancellationToken);
        return Result.Success(page.ToResponse());
    }
}
