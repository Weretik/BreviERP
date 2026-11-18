using BuildingBlocks.Domain.Identifiers.Orders;
using BuildingBlocks.Domain.Identifiers.ReferenceData;
namespace Crm.Domain.Quotes.Items;

/// <summary>
/// One fabric of a price row: a «Виріб з нуля» variant sums the cost of its fabrics (R-26). The name is copied from the
/// reference when the row is priced, so the КП reads the same after the reference changes.
/// </summary>
public sealed class QuoteItemOptionFabric
{
    private QuoteItemOptionFabric() { }

    private QuoteItemOptionFabric(int position, FabricId fabricId, string name, decimal meters)
    {
        Position = position;
        FabricId = fabricId;
        Name = name.Trim();
        Meters = meters;
    }

    public static QuoteItemOptionFabric Create(int position, FabricId fabricId, string name, decimal meters) =>
        new(position, fabricId, name, meters);

    public long Id { get; private set; }
    public QuoteOptionId OptionId { get; private set; }
    public int Position { get; private set; }

    /// <summary>Reference fabric; no foreign key, Reference owns it.</summary>
    public FabricId FabricId { get; private set; }

    public string Name { get; private set; } = null!;

    /// <summary>Metres of the fabric in one product.</summary>
    public decimal Meters { get; private set; }

    public QuoteItemOptionFabric Copy() => new(Position, FabricId, Name, Meters);
}
