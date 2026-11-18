namespace BuildingBlocks.Domain.Identifiers.Orders;

/// <summary>
/// An option (variant) of a Crm КП item. Crm owns it; Production keeps it on an order item made from it. Shared kernel (ADR-0020).
/// </summary>
public readonly record struct QuoteOptionId(long Value) : IComparable<QuoteOptionId>
{
    public int CompareTo(QuoteOptionId other) => Value.CompareTo(other.Value);

    /// <summary>The id of a value that may be missing, for a value that comes from a request or another module.</summary>
    public static QuoteOptionId? FromNullable(long? value) => value is { } id ? new QuoteOptionId(id) : null;

    public override string ToString() => Value.ToString();
}
