namespace Catalog.Application.Features.ProductCategory.Queries.GetStoreProductCategories;

public sealed record StoreProductCategoryRowDTO(
    int Id,
    string Name,
    string Slug,
    int? ParentId,
    int Level);


