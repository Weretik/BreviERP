namespace Catalog.Domain.Products.ValueObjects;

public readonly record struct AccessoryReferenceId
{
    public int Value { get; }
    private AccessoryReferenceId(int value) => Value = value;
    public static AccessoryReferenceId Create(int value) => value > 0 ? new(value) : throw new ArgumentOutOfRangeException(nameof(value));
}
