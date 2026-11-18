namespace BuildingBlocks.Domain.Identifiers.Orders;

/// <summary>
/// A КП (quote) of the Crm module. Crm owns it; Production keeps it on an order made from the КП. Shared kernel (ADR-0020).
/// </summary>
public readonly record struct QuoteId(long Value) : IComparable<QuoteId>
{
    public int CompareTo(QuoteId other) => Value.CompareTo(other.Value);

    /// <summary>The id of a value that may be missing, for a value that comes from a request or another module.</summary>
    public static QuoteId? FromNullable(long? value) => value is { } id ? new QuoteId(id) : null;

    public override string ToString() => Value.ToString();
}
