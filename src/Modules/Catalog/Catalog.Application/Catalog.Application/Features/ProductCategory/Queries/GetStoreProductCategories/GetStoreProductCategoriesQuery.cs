
namespace Catalog.Application.Features.ProductCategory.Queries.GetStoreProductCategories;

public sealed record GetStoreProductCategoriesQuery(string Language)
    : IQuery<Result<List<StoreProductCategoryRowDTO>>>;


