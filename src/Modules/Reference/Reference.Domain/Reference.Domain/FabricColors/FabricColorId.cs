namespace Reference.Domain.FabricColors;

[ValueObject<int>]
public readonly partial struct FabricColorId
{
    private static Validation Validate(int value)
    {
        if (value <= 0)
            return Validation.Invalid("Fabric colour id must be positive.");

        if (value > 1_000_000_000)
            return Validation.Invalid("Fabric colour id is too large.");

        return Validation.Ok;
    }
}
