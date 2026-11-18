namespace BuildingBlocks.Domain.Identifiers.Orders;

/// <summary>
/// An item of a Production order. Production owns it; Warehouse keeps it on fabric plans, issues and movements. Shared kernel (ADR-0020).
/// </summary>
public readonly record struct ProductionItemId(int Value) : IComparable<ProductionItemId>
{
    public int CompareTo(ProductionItemId other) => Value.CompareTo(other.Value);

    /// <summary>The id of a value that may be missing, for a value that comes from a request or another module.</summary>
    public static ProductionItemId? FromNullable(int? value) => value is { } id ? new ProductionItemId(id) : null;

    public override string ToString() => Value.ToString();
}
