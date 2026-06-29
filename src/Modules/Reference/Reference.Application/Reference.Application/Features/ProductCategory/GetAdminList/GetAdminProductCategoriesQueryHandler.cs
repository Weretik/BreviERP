using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.ProductCategory.GetAdminList.DTOs;
using Reference.Application.Features.ProductCategory.GetAdminList.Specifications;
using ProductCategoryEntity = Reference.Domain.Products.Entities.ProductCategory;

namespace Reference.Application.Features.ProductCategory.GetAdminList;

public sealed class GetAdminProductCategoriesQueryHandler(IReferenceReadRepository<ProductCategoryEntity> repository)
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
