namespace BuildingBlocks.Domain.Identifiers.Products;

/// <summary>
/// A product of the Catalog module. Catalog owns it; Content, Crm and Production keep it on their rows. Shared kernel (ADR-0020).
/// </summary>
public readonly record struct ProductId(int Value) : IComparable<ProductId>
{
    public int CompareTo(ProductId other) => Value.CompareTo(other.Value);

    /// <summary>The id of a value that may be missing, for a value that comes from a request or another module.</summary>
    public static ProductId? FromNullable(int? value) => value is { } id ? new ProductId(id) : null;

    public override string ToString() => Value.ToString();
}
