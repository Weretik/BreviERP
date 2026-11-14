using Reference.Domain.ProductSections;

namespace Reference.Application.Features.ProductSection.Shared;

/// <summary>Product type names used by the API: <c>Sewing</c> and <c>Ppe</c>.</summary>
public static class ProductSectionTypes
{
    public const string Sewing = nameof(ProductSectionType.Sewing);
    public const string Ppe = nameof(ProductSectionType.Ppe);

    public static bool IsKnown(string? value) => value is Sewing or Ppe;

    public static ProductSectionType Parse(string value) => value switch
    {
        Sewing => ProductSectionType.Sewing,
        Ppe => ProductSectionType.Ppe,
        _ => (ProductSectionType)0,
    };

    public static string Name(ProductSectionType type) => type.ToString();
}
