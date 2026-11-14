
namespace Reference.Domain.PricingParameters;

[ValueObject<int>]
public readonly partial struct PricingParameterId
{
    private static Validation Validate(int value)
    {
        if (value <= 0)
            return Validation.Invalid($"Pricing parameter id must be positive.");

        if (value > 1_000_000_000)
            return Validation.Invalid($"Pricing parameter id is too large.");

        return Validation.Ok;
    }

}
