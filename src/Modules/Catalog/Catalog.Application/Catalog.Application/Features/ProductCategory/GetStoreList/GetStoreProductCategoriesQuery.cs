using Catalog.Application.Features.ProductCategory.GetStoreList.DTOs;

namespace Catalog.Application.Features.ProductCategory.GetStoreList;

public sealed record GetStoreProductCategoriesQuery(string Language)
    : IQuery<Result<List<StoreProductCategoryRowDTO>>>;


