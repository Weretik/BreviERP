namespace BuildingBlocks.Domain.Identifiers.ReferenceData;

/// <summary>
/// A fabric of the Reference module. Reference owns it; Catalog, Crm, Production and Warehouse keep it on their rows. Shared kernel (ADR-0020).
/// </summary>
public readonly record struct FabricId(int Value) : IComparable<FabricId>
{
    public int CompareTo(FabricId other) => Value.CompareTo(other.Value);

    /// <summary>The id of a value that may be missing, for a value that comes from a request or another module.</summary>
    public static FabricId? FromNullable(int? value) => value is { } id ? new FabricId(id) : null;

    public override string ToString() => Value.ToString();
}
