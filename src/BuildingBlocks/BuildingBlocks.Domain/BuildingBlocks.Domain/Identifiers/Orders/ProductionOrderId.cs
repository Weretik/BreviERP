namespace BuildingBlocks.Domain.Identifiers.Orders;

/// <summary>
/// A production order of the Production module. Production owns it; Warehouse keeps it on fabric plans, issues and movements. Shared kernel (ADR-0020).
/// </summary>
public readonly record struct ProductionOrderId(int Value) : IComparable<ProductionOrderId>
{
    public int CompareTo(ProductionOrderId other) => Value.CompareTo(other.Value);

    /// <summary>The id of a value that may be missing, for a value that comes from a request or another module.</summary>
    public static ProductionOrderId? FromNullable(int? value) => value is { } id ? new ProductionOrderId(id) : null;

    public override string ToString() => Value.ToString();
}
