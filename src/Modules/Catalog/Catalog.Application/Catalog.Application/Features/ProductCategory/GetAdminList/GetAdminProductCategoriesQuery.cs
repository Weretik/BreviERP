using Catalog.Application.Features.ProductCategory.GetAdminList.DTOs;

namespace Catalog.Application.Features.ProductCategory.GetAdminList;

public sealed record GetAdminProductCategoriesQuery : IQuery<Result<List<AdminProductCategoryRowDTO>>>;


