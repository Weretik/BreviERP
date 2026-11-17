namespace Warehouse.Domain.Stock;

/// <summary>Identity of <see cref="FabricStock"/>; comparable, so a list sorted by id in memory keeps the database order.</summary>
public readonly record struct FabricStockId(int Value) : IComparable<FabricStockId>
{
    public int CompareTo(FabricStockId other) => Value.CompareTo(other.Value);
}
