namespace BuildingBlocks.Domain.ValueObjects;

public readonly record struct MoneyAmount
{
    public decimal Value { get; }

    public MoneyAmount(decimal value)
    {
        Value = decimal.Round(value, 2, MidpointRounding.AwayFromZero);
    }

    public static MoneyAmount Zero => new(0m);

    public static MoneyAmount From(decimal value) => new(value);

    public static implicit operator decimal(MoneyAmount amount) => amount.Value;

    public static MoneyAmount operator +(MoneyAmount left, MoneyAmount right) => new(left.Value + right.Value);

    public static MoneyAmount operator -(MoneyAmount left, MoneyAmount right) => new(left.Value - right.Value);

    public static MoneyAmount operator *(MoneyAmount amount, decimal multiplier) => new(amount.Value * multiplier);

    public static MoneyAmount operator *(decimal multiplier, MoneyAmount amount) => amount * multiplier;

    public static MoneyAmount operator /(MoneyAmount amount, decimal divisor)
    {
        if (divisor == 0m)
        {
            throw new DivideByZeroException("Money amount cannot be divided by zero.");
        }

        return new MoneyAmount(amount.Value / divisor);
    }

    public static decimal operator /(MoneyAmount left, MoneyAmount right)
    {
        if (right.Value == 0m)
        {
            throw new DivideByZeroException("Money amount cannot be divided by zero.");
        }

        return left.Value / right.Value;
    }

    public override string ToString() => Value.ToString("0.00");
}
