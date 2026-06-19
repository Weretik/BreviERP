using Reference.Application.Features.ProductCategory.GetAdminList.DTOs;

namespace Reference.Application.Features.ProductCategory.GetAdminList;

public sealed record GetAdminProductCategoriesQuery : IQuery<Result<List<AdminProductCategoryRowDTO>>>;
