namespace Warehouse.Domain.Stock.Movements.Identifiers;

/// <summary>Identity of <see cref="StockOperation"/>; comparable, so a list sorted by id in memory keeps the database order.</summary>
public readonly record struct StockOperationId(Guid Value) : IComparable<StockOperationId>
{
    public int CompareTo(StockOperationId other) => Value.CompareTo(other.Value);
}
