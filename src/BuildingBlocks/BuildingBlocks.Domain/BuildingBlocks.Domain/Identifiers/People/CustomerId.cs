namespace BuildingBlocks.Domain.Identifiers.People;

/// <summary>
/// A client of Crm. Crm owns the customer; Production keeps it on an order. Shared kernel (ADR-0020).
/// </summary>
public readonly record struct CustomerId(long Value) : IComparable<CustomerId>
{
    public int CompareTo(CustomerId other) => Value.CompareTo(other.Value);

    /// <summary>The id of a value that may be missing, for a value that comes from a request or another module.</summary>
    public static CustomerId? FromNullable(long? value) => value is { } id ? new CustomerId(id) : null;

    public override string ToString() => Value.ToString();
}
