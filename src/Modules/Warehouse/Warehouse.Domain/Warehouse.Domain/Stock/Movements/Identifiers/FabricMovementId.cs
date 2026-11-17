namespace Warehouse.Domain.Stock.Movements.Identifiers;

/// <summary>Identity of <see cref="FabricMovement"/>; comparable, so a list sorted by id in memory keeps the database order.</summary>
public readonly record struct FabricMovementId(int Value) : IComparable<FabricMovementId>
{
    public int CompareTo(FabricMovementId other) => Value.CompareTo(other.Value);
}
