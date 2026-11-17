namespace Warehouse.Domain.Stock.Plans;

/// <summary>Identity of <see cref="FabricItemPlan"/>; comparable, so a list sorted by id in memory keeps the database order.</summary>
public readonly record struct FabricItemPlanId(int Value) : IComparable<FabricItemPlanId>
{
    public int CompareTo(FabricItemPlanId other) => Value.CompareTo(other.Value);
}
