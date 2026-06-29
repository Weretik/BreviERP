namespace Reference.Domain.GarmentPartOperations.ValueObjects;

[ValueObject<int>]
public readonly partial struct GarmentPartOperationId
{
    private static Validation Validate(int value)
    {
        if (value <= 0)
            return Validation.Invalid("Garment Part Operation Id must be positive.");

        if (value > 1_000_000_000)
            return Validation.Invalid("Garment Part Operation Id is too large.");

        return Validation.Ok;
    }
}
