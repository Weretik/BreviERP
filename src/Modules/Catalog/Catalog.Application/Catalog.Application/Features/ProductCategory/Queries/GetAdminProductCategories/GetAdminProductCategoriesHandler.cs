using ProductCategoryEntity = Catalog.Domain.ProductCategories.ProductCategory;
using Catalog.Application.Abstractions;

namespace Catalog.Application.Features.ProductCategory.Queries.GetAdminProductCategories;

public sealed class GetAdminProductCategoriesHandler(ICatalogReadRepository<ProductCategoryEntity> repository)
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


