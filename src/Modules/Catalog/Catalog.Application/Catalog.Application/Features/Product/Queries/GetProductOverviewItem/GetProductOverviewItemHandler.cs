using Catalog.Application.Features.Product.Queries.GetProductOverviewPage;
using Catalog.Application.Features.Product.Shared.Overview.Items;
using Catalog.Application.Features.Product.Queries.GetProductOverviewPage.Results;

namespace Catalog.Application.Features.Product.Queries.GetProductOverviewItem;

public sealed class GetProductOverviewItemHandler(ProductOverviewItemReader reader)
    : IQueryHandler<GetProductOverviewItemQuery, Result<ProductOverviewItem>>
{
    public async ValueTask<Result<ProductOverviewItem>> Handle(
        GetProductOverviewItemQuery query, CancellationToken cancellationToken)
    {
        var items = await reader.ReadAsync(null, query.Id, cancellationToken);
        return items.Count == 0
            ? Result.NotFound()
            : Result.Success((await reader.WithProblemsAsync(items, cancellationToken))[0]);
    }
}
