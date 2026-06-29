namespace Catalog.Domain.Products.ValueObjects;

public readonly record struct ProductId
{
    public int Value { get; }

    private ProductId(int value)
    {
        Value = value;
    }

    public static ProductId Create(int value)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(nameof(value), value, "Product Id must be positive.");

        if (value > 1_000_000_000)
            throw new ArgumentOutOfRangeException(nameof(value), value, "Product Id is too large.");

        return new ProductId(value);
    }

    public override string ToString() => Value.ToString();

    public static implicit operator int(ProductId id) => id.Value;
}
