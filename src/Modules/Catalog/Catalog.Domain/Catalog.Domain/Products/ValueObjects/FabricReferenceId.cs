namespace Catalog.Domain.Products.ValueObjects;

public readonly record struct FabricReferenceId
{
    public int Value { get; }
    private FabricReferenceId(int value) => Value = value;
    public static FabricReferenceId Create(int value) => value > 0 ? new(value) : throw new ArgumentOutOfRangeException(nameof(value));
}
