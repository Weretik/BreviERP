namespace Production.Domain.FabricNorms;

/// <summary>
/// Planned fabric per item of a model, in metres, for an order sewn in one, two or three colours. With two colours the
/// main fabric and the additional one are planned separately; with three, the main and two additional ones. Totals are
/// the sum of their parts. Every value is optional: a workshop may not have measured a variant yet.
/// </summary>
public sealed record FabricNormValues(
    decimal? OneColor,
    decimal? TwoColorsMain,
    decimal? TwoColorsAdditional,
    decimal? ThreeColorsMain,
    decimal? ThreeColorsAdditional1,
    decimal? ThreeColorsAdditional2)
{
    /// <summary>More than any real garment needs; guards against a typo in centimetres.</summary>
    public const decimal MaxMeters = 100m;

    public const int Decimals = 3;

    public static FabricNormValues Empty { get; } = new(null, null, null, null, null, null);

    public bool IsEmpty => this == Empty;

    public decimal? TwoColorsTotal => Sum(TwoColorsMain, TwoColorsAdditional);

    public decimal? ThreeColorsTotal => Sum(ThreeColorsMain, ThreeColorsAdditional1, ThreeColorsAdditional2);

    public IEnumerable<decimal?> All()
    {
        yield return OneColor;
        yield return TwoColorsMain;
        yield return TwoColorsAdditional;
        yield return ThreeColorsMain;
        yield return ThreeColorsAdditional1;
        yield return ThreeColorsAdditional2;
    }

    public static bool IsValidMeters(decimal? value) =>
        value is null || value > 0 && value <= MaxMeters && decimal.Round(value.Value, Decimals) == value.Value;

    /// <summary>A part of a split norm needs its main fabric; additional fabric alone is not a plan.</summary>
    public bool HasMainForEveryAdditional =>
        (TwoColorsAdditional is null || TwoColorsMain is not null)
        && (ThreeColorsAdditional1 is null && ThreeColorsAdditional2 is null || ThreeColorsMain is not null);

    private static decimal? Sum(params decimal?[] parts) =>
        parts.All(x => x is null) ? null : parts.Sum(x => x ?? 0m);
}
