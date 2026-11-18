namespace BuildingBlocks.Domain.Identifiers.ReferenceData;

/// <summary>
/// A supplier of the Reference module. Reference owns it; Catalog, Purchasing and Warehouse keep it on their rows. Shared kernel (ADR-0020).
/// </summary>
public readonly record struct SupplierId(int Value) : IComparable<SupplierId>
{
    public int CompareTo(SupplierId other) => Value.CompareTo(other.Value);

    /// <summary>The id of a value that may be missing, for a value that comes from a request or another module.</summary>
    public static SupplierId? FromNullable(int? value) => value is { } id ? new SupplierId(id) : null;

    public override string ToString() => Value.ToString();
}
