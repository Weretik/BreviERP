using Reference.Application.Features.ProductCategory.GetAdminList.DTOs;
using ProductCategoryEntity = Reference.Domain.Products.Entities.ProductCategory;

namespace Reference.Application.Features.ProductCategory.GetAdminList.Specifications;

public sealed class GetAdminProductCategoriesSpec : Specification<ProductCategoryEntity, AdminProductCategoryRowDTO>
{
    public GetAdminProductCategoriesSpec()
    {
        Query.AsNoTracking()
            .OrderBy(x => x.Path)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .Select(x => new AdminProductCategoryRowDTO(
                x.Id.Value,
                x.Name,
                x.RuName,
                x.Slug,
                x.ParentId.HasValue ? x.ParentId.Value.Value : null,
                x.Path,
                x.Level,
                x.SortOrder,
                x.IsActive,
                x.Description));
    }
}
