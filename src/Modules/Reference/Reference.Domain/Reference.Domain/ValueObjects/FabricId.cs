using Domain.Common.Helpers;

namespace Reference.Domain.ValueObjects;

[ValueObject<int>]
public readonly partial struct FabricId
{
    private static Validation Validate(int value)
    {
        if (value <= 0)
            return Validation.Invalid($"Fabric Id must be positive.");

        if (value > 1_000_000_000)
            return Validation.Invalid($"Fabric Id is too large.");

        return Validation.Ok;
    }

}
