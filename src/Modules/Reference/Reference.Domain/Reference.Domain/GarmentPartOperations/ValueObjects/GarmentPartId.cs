namespace Reference.Domain.GarmentPartOperations.ValueObjects;

[ValueObject<int>]
public readonly partial struct GarmentPartId
{
    private static Validation Validate(int value)
    {
        if (value <= 0)
            return Validation.Invalid("Garment Part Id must be positive.");

        if (value > 1_000_000_000)
            return Validation.Invalid("Garment Part Id is too large.");

        return Validation.Ok;
    }
}
