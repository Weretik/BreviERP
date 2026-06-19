namespace Reference.Application.Features.ProductCategory.GetAdminList.DTOs;

public sealed record AdminProductCategoryRowDTO(
    int Id,
    string Name,
    string RuName,
    string Slug,
    int? ParentId,
    string Path,
    int Level,
    int SortOrder,
    bool IsActive,
    string? Description);
