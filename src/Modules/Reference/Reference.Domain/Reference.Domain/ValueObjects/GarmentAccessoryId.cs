namespace Reference.Domain.ValueObjects;

[ValueObject<int>]
public readonly partial struct GarmentAccessoryId
{
    private static Validation Validate(int value)
    {
        if (value <= 0)
            return Validation.Invalid("Garment accessory Id must be positive.");

        if (value > 1_000_000_000)
            return Validation.Invalid("Garment accessory Id is too large.");

        return Validation.Ok;
    }
}
