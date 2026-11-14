using Catalog.Domain.Products;
using ProductEntity = Catalog.Domain.Products.Product;
using Catalog.Application.Abstractions;
using Catalog.Application.Features.Product.Shared.Overview;
using Catalog.Application.Features.Product.Shared.Photos;

namespace Catalog.Application.Features.Product.Queries.GetProductSkuIndex;

public sealed class GetProductSkuIndexHandler(ICatalogReadRepository<ProductEntity> repository)
    : IQueryHandler<GetProductSkuIndexQuery, Result<IReadOnlyList<ProductSkuIndexItem>>>
{
    public async ValueTask<Result<IReadOnlyList<ProductSkuIndexItem>>> Handle(
        GetProductSkuIndexQuery query, CancellationToken cancellationToken)
    {
        var rows = await repository.ListAsync(new ProductSkuIndexSpec(), cancellationToken);
        IReadOnlyList<ProductSkuIndexItem> items = rows.Select(row => new ProductSkuIndexItem(
            row.Id, ProductOverviewTypes.Name(row.Type), row.Name, row.IsActive,
            (row.Type == ProductType.Ppe ? [row.SaleSku] : row.ArticleSkus.Cast<string?>())
                .Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x!.Trim()).Distinct().ToList())).ToList();
        return Result.Success(items);
    }
}
