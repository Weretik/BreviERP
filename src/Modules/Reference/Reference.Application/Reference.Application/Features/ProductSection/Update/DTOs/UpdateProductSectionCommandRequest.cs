using Reference.Application.Features.ProductSection.Shared.DTOs;

namespace Reference.Application.Features.ProductSection.Update.DTOs;

public sealed record UpdateProductSectionCommandRequest(
    string Type,
    string Name,
    bool IsActive = true,
    string? Comment = null,
    int SortOrder = 0) : IProductSectionFields;
