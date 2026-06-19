namespace Reference.Application.Features.ProductCategory.GetStoreList.DTOs;

public sealed record StoreProductCategoryRowDTO(
    int Id,
    string Name,
    string Slug,
    int? ParentId,
    int Level);
