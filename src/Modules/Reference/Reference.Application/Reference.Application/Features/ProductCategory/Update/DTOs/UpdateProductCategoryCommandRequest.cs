namespace Reference.Application.Features.ProductCategory.Update.DTOs;

public sealed record UpdateProductCategoryCommandRequest(
    string Name,
    string RuName,
    string Slug,
    int? ParentId = null,
    int SortOrder = 0,
    bool IsActive = true,
    string? Description = null);
