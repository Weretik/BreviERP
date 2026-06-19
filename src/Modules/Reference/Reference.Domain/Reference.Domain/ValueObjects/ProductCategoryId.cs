namespace Reference.Domain.ValueObjects;

[ValueObject<int>]
public readonly partial struct ProductCategoryId
{
    private static Validation Validate(int value)
    {
        if (value <= 0)
            return Validation.Invalid("Product Category Id must be positive.");

        if (value > 1_000_000_000)
            return Validation.Invalid("Product Category Id is too large.");

        return Validation.Ok;
    }
}
