using Reference.Application.Features.ProductCategory.GetStoreList.DTOs;

namespace Reference.Application.Features.ProductCategory.GetStoreList;

public sealed record GetStoreProductCategoriesQuery(string Language)
    : IQuery<Result<List<StoreProductCategoryRowDTO>>>;
