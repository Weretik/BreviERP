namespace Domain.Reference.ValueObjects;

[ValueObject<decimal>]
public readonly partial struct Percent
{
    private static Validation Validate(decimal value) =>
     value is >= 0m and <= 100m
         ? Validation.Ok
         : Validation.Invalid("Percent must be between 0 and 100.");

    // 15% of 2000 = 300
    public decimal Of(decimal amount) => amount * Value / 100m;

    // 2000 + 15% = 2300
    public decimal ApplyChange(decimal amount) => amount * (1m + Value / 100m);

    // 2000 - 15% = 1700
    public decimal ApplyDiscount(decimal amount) => amount * (1m - Value / 100m);
}
