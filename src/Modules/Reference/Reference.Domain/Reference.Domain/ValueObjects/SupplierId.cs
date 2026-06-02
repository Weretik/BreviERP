namespace Reference.Domain.ValueObjects;

[ValueObject<int>]
public readonly partial struct SupplierId
{
    private static Validation Validate(int value)
    {
        if (value <= 0)
            return Validation.Invalid("Supplier Id must be positive.");

        if (value > 1_000_000_000)
            return Validation.Invalid("Supplier Id is too large.");

        return Validation.Ok;
    }
}
