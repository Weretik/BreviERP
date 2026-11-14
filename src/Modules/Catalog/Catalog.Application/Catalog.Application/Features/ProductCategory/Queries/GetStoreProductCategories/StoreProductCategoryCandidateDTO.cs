namespace Catalog.Application.Features.ProductCategory.Queries.GetStoreProductCategories;

public sealed record StoreProductCategoryCandidateDTO(
    int Id,
    string Name,
    string Slug,
    int? ParentId,
    string Path,
    int Level,
    bool IsActive);


