namespace Reference.Domain.PpeMarkupRules;

[ValueObject<int>]
public readonly partial struct PpeMarkupRuleId
{
    private static Validation Validate(int value)
    {
        if (value <= 0)
            return Validation.Invalid("PPE markup rule id must be positive.");

        if (value > 1_000_000_000)
            return Validation.Invalid("PPE markup rule id is too large.");

        return Validation.Ok;
    }
}
