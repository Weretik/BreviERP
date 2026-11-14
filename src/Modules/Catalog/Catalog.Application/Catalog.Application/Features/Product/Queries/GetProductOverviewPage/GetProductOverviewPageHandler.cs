using Catalog.Application.Features.Product.Shared.Overview.Items;

namespace Catalog.Application.Features.Product.Queries.GetProductOverviewPage;

public sealed class GetProductOverviewPageHandler(ProductOverviewItemReader reader)
    : IQueryHandler<GetProductOverviewPageQuery, Result<ProductOverviewPage>>
{
    public async ValueTask<Result<ProductOverviewPage>> Handle(
        GetProductOverviewPageQuery query, CancellationToken cancellationToken)
    {
        var all = await reader.ReadAsync(query.Type, null, cancellationToken);
        var matching = ProductOverviewListing.Sort(ProductOverviewListing.Filter(all, query), query.Sort,
            query.Descending).ToList();
        var page = matching.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToList();
        return Result.Success(new ProductOverviewPage(
            await reader.WithProblemsAsync(page, cancellationToken),
            query.Page, query.PageSize, matching.Count, all.Count,
            ProductOverviewListing.CountSections(all),
            all.Count(x => x.Prices is null),
            all.Count(x => !x.IsActive)));
    }
}
