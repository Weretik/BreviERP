namespace BuildingBlocks.Domain.Identifiers.ReferenceData;

/// <summary>
/// A sewing operation of the Reference module. Reference owns it; Catalog keeps it in a sewing composition. Shared kernel (ADR-0020).
/// </summary>
public readonly record struct GarmentPartOperationId(int Value) : IComparable<GarmentPartOperationId>
{
    public int CompareTo(GarmentPartOperationId other) => Value.CompareTo(other.Value);

    /// <summary>The id of a value that may be missing, for a value that comes from a request or another module.</summary>
    public static GarmentPartOperationId? FromNullable(int? value) => value is { } id ? new GarmentPartOperationId(id) : null;

    public override string ToString() => Value.ToString();
}
