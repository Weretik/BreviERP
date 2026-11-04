namespace Crm.Domain.Quotes;

/// <summary>
/// One price of a row. <see cref="Source"/> is the Catalog price copied into the КП (null for a one-off PPE);
/// <see cref="Manual"/> is the manager's own price (PPE only). The client sees <see cref="Amount"/> when
/// <see cref="Shown"/> is set.
/// </summary>
public sealed record QuotePrice(decimal? Source, decimal? Manual, bool Shown)
{
    public static readonly QuotePrice Empty = new(null, null, false);

    public decimal? Amount => Manual ?? Source;

    public bool IsManual => Manual is not null && Manual != Source;

    public bool IsVisiblePositive => Shown && Amount is > 0;

    public QuotePrice WithSource(decimal? source) => this with { Source = source };
}
