using Crm.Domain.Quotes.Enums;

namespace Crm.Domain.Quotes.Items;

/// <summary>
/// An accessory, operation, service or extra fabric of a «Виріб з нуля» or a changed Catalog model. The name and unit
/// are copied from the reference when the line is added, so the КП reads the same after the reference changes.
/// </summary>
public sealed class QuoteItemLine
{
    private QuoteItemLine() { }

    public QuoteItemLine(
        int position, QuoteItemLineKind kind, int referenceId, string name, string unit, decimal quantity,
        decimal? manualMinutes, string? costCode = null)
    {
        Position = position;
        Kind = kind;
        ReferenceId = referenceId;
        Name = name.Trim();
        Unit = unit.Trim();
        Quantity = quantity;
        ManualMinutes = kind == QuoteItemLineKind.Operation ? manualMinutes : null;
        CostCode = kind == QuoteItemLineKind.Service ? costCode?.Trim() : null;
        if (kind == QuoteItemLineKind.Service) ReferenceId = 0;
    }

    public long Id { get; private set; }
    public long ItemId { get; private set; }
    public int Position { get; private set; }
    public QuoteItemLineKind Kind { get; private set; }

    /// <summary>Reference garment accessory, operation or fabric; 0 for a service.</summary>
    public int ReferenceId { get; private set; }

    public string Name { get; private set; } = null!;
    public string Unit { get; private set; } = null!;
    public decimal Quantity { get; private set; }

    /// <summary>Minutes of an operation without a fixed norm.</summary>
    public decimal? ManualMinutes { get; private set; }

    /// <summary>The Reference other-cost code of a service.</summary>
    public string? CostCode { get; private set; }

    public QuoteItemLine Copy() =>
        new(Position, Kind, ReferenceId, Name, Unit, Quantity, ManualMinutes, CostCode);

    public bool SameAs(QuoteItemLine other) =>
        Kind == other.Kind && ReferenceId == other.ReferenceId && CostCode == other.CostCode &&
        Quantity == other.Quantity && ManualMinutes == other.ManualMinutes;
}
