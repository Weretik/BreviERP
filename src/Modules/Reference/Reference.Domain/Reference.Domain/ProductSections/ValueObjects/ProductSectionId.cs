namespace Reference.Domain.ProductSections.ValueObjects;

[ValueObject<int>]
public readonly partial struct ProductSectionId
{
    private static Validation Validate(int value)
    {
        if (value <= 0)
            return Validation.Invalid("Product section id must be positive.");

        if (value > 1_000_000_000)
            return Validation.Invalid("Product section id is too large.");

        return Validation.Ok;
    }
}
