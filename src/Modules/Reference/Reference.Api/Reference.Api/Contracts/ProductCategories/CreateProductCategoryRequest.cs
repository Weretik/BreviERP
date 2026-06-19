namespace Reference.Api.Contracts.ProductCategories;

public sealed record CreateProductCategoryRequest(
    int Id,
    string Name,
    string RuName,
    string Slug,
    int? ParentId = null,
    int SortOrder = 0,
    bool IsActive = true,
    string? Description = null);
