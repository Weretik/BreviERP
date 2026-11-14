namespace Crm.Domain.Quotes.ValueObjects;

/// <summary>`n-month-year` (R-03): the n-th КП of that Kyiv month. Never changes once assigned.</summary>
public sealed record QuoteNumber(int Year, int Month, int Sequence)
{
    public override string ToString() => $"{Sequence}-{Month}-{Year}";
}
