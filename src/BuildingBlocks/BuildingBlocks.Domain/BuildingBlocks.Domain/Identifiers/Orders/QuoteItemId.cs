namespace BuildingBlocks.Domain.Identifiers.Orders;

/// <summary>
/// An item of a Crm КП. Crm owns it; Production keeps it on an order item made from it. Shared kernel (ADR-0020).
/// </summary>
public readonly record struct QuoteItemId(long Value) : IComparable<QuoteItemId>
{
    public int CompareTo(QuoteItemId other) => Value.CompareTo(other.Value);

    /// <summary>The id of a value that may be missing, for a value that comes from a request or another module.</summary>
    public static QuoteItemId? FromNullable(long? value) => value is { } id ? new QuoteItemId(id) : null;

    public override string ToString() => Value.ToString();
}
