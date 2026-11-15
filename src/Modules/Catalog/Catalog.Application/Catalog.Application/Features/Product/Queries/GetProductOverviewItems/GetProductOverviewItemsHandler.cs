using Catalog.Application.Features.Product.Shared.Overview.Items;
using Catalog.Application.Features.Product.Queries.GetProductOverviewPage.Results;

namespace Catalog.Application.Features.Product.Queries.GetProductOverviewItems;

/// <summary>Reads the products in a constant number of queries, whatever their count; missing ids are left out.</summary>
public sealed class GetProductOverviewItemsHandler(ProductOverviewItemReader reader)
    : IQueryHandler<GetProductOverviewItemsQuery, Result<IReadOnlyList<ProductOverviewItem>>>
{
    public async ValueTask<Result<IReadOnlyList<ProductOverviewItem>>> Handle(GetProductOverviewItemsQuery query,
        CancellationToken cancellationToken)
    {
        if (query.Ids.Count == 0) return Result.Success<IReadOnlyList<ProductOverviewItem>>([]);
        var items = await reader.ReadAsync(null, query.Ids, cancellationToken);
        return Result.Success(await reader.WithProblemsAsync(items, cancellationToken));
    }
}
