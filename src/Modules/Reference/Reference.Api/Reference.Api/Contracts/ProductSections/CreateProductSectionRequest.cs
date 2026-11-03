namespace Reference.Api.Contracts.ProductSections;

/// <summary>A new product section; <paramref name="Type"/> is <c>Sewing</c> or <c>Ppe</c>.</summary>
public sealed record CreateProductSectionRequest(
    string Type,
    string Name,
    bool IsActive = true,
    string? Comment = null,
    int SortOrder = 0);
