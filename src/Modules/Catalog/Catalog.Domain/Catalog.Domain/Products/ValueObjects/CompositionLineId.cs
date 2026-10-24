namespace Catalog.Domain.Products.ValueObjects;

public readonly record struct CompositionLineId
{
    public long Value { get; }
    private CompositionLineId(long value) => Value = value;
    public static CompositionLineId Create(long value) => value < 0
        ? throw new ArgumentOutOfRangeException(nameof(value), "Composition line ID cannot be negative.")
        : new CompositionLineId(value);
    internal static CompositionLineId CreateTemporary(long value) => value < 0
        ? new CompositionLineId(value)
        : throw new ArgumentOutOfRangeException(nameof(value), "Temporary composition line IDs must be negative.");
}
