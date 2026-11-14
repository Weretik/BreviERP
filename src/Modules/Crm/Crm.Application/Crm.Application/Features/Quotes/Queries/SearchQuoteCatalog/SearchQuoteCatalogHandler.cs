using Crm.Domain.Quotes;
using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Shared.Catalog;
using Crm.Application.Features.Quotes.Shared.Introductions.Building;
using Crm.Application.Features.Quotes.Shared.Mapping;
using Crm.Application.Abstractions.Quotes;
using Crm.Domain.Quotes.Enums;
using Crm.Application.Abstractions.Quotes.Products;

namespace Crm.Application.Features.Quotes.Queries.SearchQuoteCatalog;

public sealed class SearchQuoteCatalogHandler(IQuoteCatalogReader catalog)
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
