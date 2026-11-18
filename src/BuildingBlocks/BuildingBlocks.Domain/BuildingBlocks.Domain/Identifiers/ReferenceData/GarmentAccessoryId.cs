namespace BuildingBlocks.Domain.Identifiers.ReferenceData;

/// <summary>
/// A garment accessory (фурнітура) of the Reference module. Reference owns it; Catalog keeps it in a sewing composition. Shared kernel (ADR-0020).
/// </summary>
public readonly record struct GarmentAccessoryId(int Value) : IComparable<GarmentAccessoryId>
{
    public int CompareTo(GarmentAccessoryId other) => Value.CompareTo(other.Value);

    /// <summary>The id of a value that may be missing, for a value that comes from a request or another module.</summary>
    public static GarmentAccessoryId? FromNullable(int? value) => value is { } id ? new GarmentAccessoryId(id) : null;

    public override string ToString() => Value.ToString();
}
