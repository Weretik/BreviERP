namespace Catalog.Domain.ProductCategories.ValueObjects;

public readonly record struct ProductCategoryId
{
    public int Value { get; }

    private ProductCategoryId(int value)
    {
        Value = value;
    }

    public static ProductCategoryId Create(int value)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(nameof(value), value, "Product category id must be positive.");

        if (value > 1_000_000_000)
            throw new ArgumentOutOfRangeException(nameof(value), value, "Product category id is too large.");

        return new ProductCategoryId(value);
    }

    public override string ToString() => Value.ToString();

    public static implicit operator int(ProductCategoryId id) => id.Value;
}