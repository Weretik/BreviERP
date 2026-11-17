namespace Warehouse.Domain.Stock.Movements.Identifiers;

/// <summary>Identity of <see cref="FabricMovementRevision"/>; comparable, so a list sorted by id in memory keeps the database order.</summary>
public readonly record struct FabricMovementRevisionId(int Value) : IComparable<FabricMovementRevisionId>
{
    public int CompareTo(FabricMovementRevisionId other) => Value.CompareTo(other.Value);
}
