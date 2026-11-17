namespace Warehouse.Domain.Stock.Movements.Identifiers;

/// <summary>Identity of <see cref="StockTransfer"/>; comparable, so a list sorted by id in memory keeps the database order.</summary>
public readonly record struct StockTransferId(int Value) : IComparable<StockTransferId>
{
    public int CompareTo(StockTransferId other) => Value.CompareTo(other.Value);
}
