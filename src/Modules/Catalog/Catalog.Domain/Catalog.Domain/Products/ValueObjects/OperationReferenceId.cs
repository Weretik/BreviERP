namespace Catalog.Domain.Products.ValueObjects;

public readonly record struct OperationReferenceId
{
    public int Value { get; }
    private OperationReferenceId(int value) => Value = value;
    public static OperationReferenceId Create(int value) => value > 0 ? new(value) : throw new ArgumentOutOfRangeException(nameof(value));
}
