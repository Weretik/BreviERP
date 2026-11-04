namespace Crm.Domain.Quotes;

/// <summary>
/// An accessory or operation of a «Виріб з нуля». The name and unit are copied from the reference when the line is
/// added, so the КП reads the same after the reference changes.
/// </summary>
public sealed class QuoteItemLine
{
    private QuoteItemLine() { }

    public QuoteItemLine(
        int position, QuoteItemLineKind kind, int referenceId, string name, string unit, decimal quantity,
        decimal? manualMinutes)
    {
        Position = position;
        Kind = kind;
        ReferenceId = referenceId;
        Name = name.Trim();
        Unit = unit.Trim();
        Quantity = quantity;
        ManualMinutes = kind == QuoteItemLineKind.Operation ? manualMinutes : null;
    }

    public long Id { get; private set; }
    public long ItemId { get; private set; }
    public int Position { get; private set; }
    public QuoteItemLineKind Kind { get; private set; }

    /// <summary>Reference garment accessory or operation.</summary>
    public int ReferenceId { get; private set; }

    public string Name { get; private set; } = null!;
    public string Unit { get; private set; } = null!;
    public decimal Quantity { get; private set; }

    /// <summary>Minutes of an operation without a fixed norm.</summary>
    public decimal? ManualMinutes { get; private set; }

    public bool SameAs(QuoteItemLine other) =>
        Kind == other.Kind && ReferenceId == other.ReferenceId && Quantity == other.Quantity &&
        ManualMinutes == other.ManualMinutes;
}
