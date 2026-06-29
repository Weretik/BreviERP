namespace Reference.Application.Features.ProductCategory.GetStoreList.DTOs;

public sealed record StoreProductCategoryCandidateDTO(
    int Id,
    string Name,
    string Slug,
    int? ParentId,
    string Path,
    int Level,
    bool IsActive);
