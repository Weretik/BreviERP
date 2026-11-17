namespace Warehouse.Domain.Stock.Plans;

/// <summary>Identity of <see cref="FabricIssue"/>; comparable, so a list sorted by id in memory keeps the database order.</summary>
public readonly record struct FabricIssueId(int Value) : IComparable<FabricIssueId>
{
    public int CompareTo(FabricIssueId other) => Value.CompareTo(other.Value);
}
