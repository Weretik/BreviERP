using Domain.Common.Helpers;

namespace Domain.Reference.ValueObjects;

[ValueObject<int>]
public readonly partial struct FabricId
{
    private static Validation Validate(int value)
        => IntIdValidator.ValidateIntId(value, nameof(FabricId));
}
