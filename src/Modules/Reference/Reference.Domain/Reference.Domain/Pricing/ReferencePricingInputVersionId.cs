namespace Reference.Domain.Pricing;

public readonly record struct ReferencePricingInputVersionId
{
    public long Value { get; }
    private ReferencePricingInputVersionId(long value) => Value = value;
    public static ReferencePricingInputVersionId Create(long value) => value >= 0
        ? new(value) : throw new ArgumentOutOfRangeException(nameof(value));
    internal static ReferencePricingInputVersionId Temporary(long value) => new(value);
}

