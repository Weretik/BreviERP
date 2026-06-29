namespace Catalog.Domain.Products.ValueObjects;

public readonly record struct ProductPhotoId
{
    public int Value { get; }

    private ProductPhotoId(int value)
    {
        Value = value;
    }

    public static ProductPhotoId Create(int value)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(nameof(value), value, "Product photo id must be positive.");

        if (value > 1_000_000_000)
            throw new ArgumentOutOfRangeException(nameof(value), value, "Product photo id is too large.");

        return new ProductPhotoId(value);
    }

    public override string ToString() => Value.ToString();

    public static implicit operator int(ProductPhotoId id) => id.Value;
}
