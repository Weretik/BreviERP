using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.ProductCategory.GetAdminList.DTOs;
using Catalog.Application.Features.ProductCategory.GetAdminList.Specifications;
using ProductCategoryEntity = Catalog.Domain.ProductCategories.Entities.ProductCategory;

namespace Catalog.Application.Features.ProductCategory.GetAdminList;

public sealed class GetAdminProductCategoriesQueryHandler(ICatalogReadRepository<ProductCategoryEntity> repository)
    : IQueryHandler<GetAdminProductCategoriesQuery, Result<List<AdminProductCategoryRowDTO>>>
{
    public async ValueTask<Result<List<AdminProductCategoryRowDTO>>> Handle(
        GetAdminProductCategoriesQuery query,
        CancellationToken cancellationToken)
    {
        var result = await repository.ListAsync(new GetAdminProductCategoriesSpec(), cancellationToken);

        if (result is { Count: 0 })
            return Result.NotFound();

        return Result.Success(result);
    }
}


