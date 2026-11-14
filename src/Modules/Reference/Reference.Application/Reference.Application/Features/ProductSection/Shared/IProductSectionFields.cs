namespace Reference.Application.Features.ProductSection.Shared;

/// <summary>Editable fields of a product section; <see cref="Type"/> is <c>Sewing</c> or <c>Ppe</c>.</summary>
public interface IProductSectionFields
{
    string Type { get; }
    string Name { get; }
    bool IsActive { get; }
    string? Comment { get; }
    int SortOrder { get; }
}
