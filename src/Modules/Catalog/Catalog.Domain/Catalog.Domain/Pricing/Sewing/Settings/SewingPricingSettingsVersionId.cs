namespace Catalog.Domain.Pricing.Sewing;

public readonly record struct SewingPricingSettingsVersionId
{
    public long Value { get; }
    private SewingPricingSettingsVersionId(long value) => Value = value;
    public static SewingPricingSettingsVersionId Create(long value) => value >= 0
        ? new(value) : throw new ArgumentOutOfRangeException(nameof(value));
    internal static SewingPricingSettingsVersionId Temporary(long value) => new(value);
}

