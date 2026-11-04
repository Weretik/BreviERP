using Catalog.Application.Features.Product.Overview.GetList.DTOs;
using Catalog.Application.Features.Product.Overview.Shared.Items;

namespace Catalog.Application.Features.Product.Overview.GetOne;

public sealed class GetProductOverviewItemQueryHandler(ProductOverviewItemReader reader)
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
