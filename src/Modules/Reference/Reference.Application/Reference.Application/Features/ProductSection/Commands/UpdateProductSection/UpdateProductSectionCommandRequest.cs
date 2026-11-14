using Reference.Application.Features.ProductSection.Shared;

namespace Reference.Application.Features.ProductSection.Commands.UpdateProductSection;

public sealed record UpdateProductSectionCommandRequest(
    string Type,
    string Name,
    bool IsActive = true,
    string? Comment = null,
    int SortOrder = 0) : IProductSectionFields;
