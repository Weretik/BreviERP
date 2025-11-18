namespace Domain.CRM.ValueObjects;

[ValueObject<int>]
public partial struct CounterpartyId
{
    private static Validation Validate(int value)
    {
        if (value <= 0) return Validation.Invalid("Id must be positive.");
        if (value > 1_000_000_000) return Validation.Invalid("Id is too large.");
        return Validation.Ok;
    }
};
