using Reference.Application.Features.ProductSection.Shared;

namespace Reference.Application.Features.ProductSection.Commands.CreateProductSection;

public sealed record CreateProductSectionCommandRequest(
    string Type,
    string Name,
    bool IsActive = true,
    string? Comment = null,
    int SortOrder = 0) : IProductSectionFields;
