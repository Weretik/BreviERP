namespace BuildingBlocks.Domain.Identifiers.ReferenceData;

/// <summary>
/// A product section of the Reference module. Reference owns it; Catalog keeps it on a product. Shared kernel (ADR-0020).
/// </summary>
public readonly record struct ProductSectionId(int Value) : IComparable<ProductSectionId>
{
    public int CompareTo(ProductSectionId other) => Value.CompareTo(other.Value);

    /// <summary>The id of a value that may be missing, for a value that comes from a request or another module.</summary>
    public static ProductSectionId? FromNullable(int? value) => value is { } id ? new ProductSectionId(id) : null;

    public override string ToString() => Value.ToString();
}
