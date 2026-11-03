using Reference.Application.Features.ProductSection.Shared.DTOs;

namespace Reference.Application.Features.ProductSection.Create.DTOs;

public sealed record CreateProductSectionCommandRequest(
    string Type,
    string Name,
    bool IsActive = true,
    string? Comment = null,
    int SortOrder = 0) : IProductSectionFields;
