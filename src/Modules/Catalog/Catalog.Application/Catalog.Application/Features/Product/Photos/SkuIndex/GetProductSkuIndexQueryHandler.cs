using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.Product.Overview.Shared;
using Catalog.Application.Features.Product.Photos.DTOs;
using Catalog.Application.Features.Product.Photos.Specifications;
using Catalog.Domain.Products.Enums;
using ProductEntity = Catalog.Domain.Products.Entities.Product;

namespace Catalog.Application.Features.Product.Photos.SkuIndex;

public sealed class GetProductSkuIndexQueryHandler(ICatalogReadRepository<ProductEntity> repository)
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
