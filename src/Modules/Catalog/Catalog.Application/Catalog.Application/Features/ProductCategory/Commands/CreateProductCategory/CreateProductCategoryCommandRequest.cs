namespace Catalog.Application.Features.ProductCategory.Commands.CreateProductCategory;

public sealed record CreateProductCategoryCommandRequest(
    int Id,
    string Name,
    string RuName,
    string Slug,
    int? ParentId = null,
    int SortOrder = 0,
    bool IsActive = true,
    string? Description = null);


