namespace Catalog.Domain.Products.ValueObjects;

public readonly record struct FabricReferenceId
{
    public int Value { get; }
    private FabricReferenceId(int value) => Value = value;
    public static FabricReferenceId Create(int value) => value > 0 ? new(value) : throw new ArgumentOutOfRangeException(nameof(value));
}
public readonly record struct AccessoryReferenceId
{
    public int Value { get; }
    private AccessoryReferenceId(int value) => Value = value;
    public static AccessoryReferenceId Create(int value) => value > 0 ? new(value) : throw new ArgumentOutOfRangeException(nameof(value));
}
public readonly record struct OperationReferenceId
{
    public int Value { get; }
    private OperationReferenceId(int value) => Value = value;
    public static OperationReferenceId Create(int value) => value > 0 ? new(value) : throw new ArgumentOutOfRangeException(nameof(value));
}
