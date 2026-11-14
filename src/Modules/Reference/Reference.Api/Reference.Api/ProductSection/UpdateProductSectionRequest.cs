namespace Reference.Api.ProductSection;

/// <summary>Editable fields of a product section; <paramref name="Type"/> is <c>Sewing</c> or <c>Ppe</c>.</summary>
public sealed record UpdateProductSectionRequest(
    string Type,
    string Name,
    bool IsActive = true,
    string? Comment = null,
    int SortOrder = 0);
