namespace Catalog.Domain.Products.ValueObjects;

public readonly record struct SupplierReferenceId
{
    public int Value { get; }

    private SupplierReferenceId(int value) => Value = value;

    public static SupplierReferenceId Create(int value)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
        return new SupplierReferenceId(value);
    }
}
