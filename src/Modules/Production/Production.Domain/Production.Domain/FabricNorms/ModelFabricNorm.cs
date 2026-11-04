namespace Production.Domain.FabricNorms;

/// <summary>
/// «Норми витрати тканини»: how much fabric one item of a Catalog sewing model needs. Owned by the workshop and keyed by
/// the model code (MOD-…); the model itself stays in Catalog. Orders of later stages plan and book fabric from it.
/// </summary>
public sealed class ModelFabricNorm
{
    public const int ModelCodeMaxLength = 32;

    private ModelFabricNorm() { }

    public ModelFabricNorm(string modelCode, FabricNormValues values, int? userId, DateTimeOffset now)
    {
        ModelCode = NormalizeCode(modelCode);
        Set(values, userId, now);
    }

    public int Id { get; private set; }
    public string ModelCode { get; private set; } = null!;
    public decimal? OneColor { get; private set; }
    public decimal? TwoColorsMain { get; private set; }
    public decimal? TwoColorsAdditional { get; private set; }
    public decimal? ThreeColorsMain { get; private set; }
    public decimal? ThreeColorsAdditional1 { get; private set; }
    public decimal? ThreeColorsAdditional2 { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public int? UpdatedByUserId { get; private set; }

    public FabricNormValues Values => new(OneColor, TwoColorsMain, TwoColorsAdditional, ThreeColorsMain,
        ThreeColorsAdditional1, ThreeColorsAdditional2);

    /// <summary>False when nothing changed, so the change time stays.</summary>
    public bool Set(FabricNormValues values, int? userId, DateTimeOffset now)
    {
        if (values.All().Any(x => !FabricNormValues.IsValidMeters(x)))
            throw new ArgumentException("Norm values must be positive metres with at most three decimals.", nameof(values));
        if (!values.HasMainForEveryAdditional)
            throw new ArgumentException("Additional fabric needs its main fabric.", nameof(values));
        if (UpdatedAt != default && Values == values) return false;

        OneColor = values.OneColor;
        TwoColorsMain = values.TwoColorsMain;
        TwoColorsAdditional = values.TwoColorsAdditional;
        ThreeColorsMain = values.ThreeColorsMain;
        ThreeColorsAdditional1 = values.ThreeColorsAdditional1;
        ThreeColorsAdditional2 = values.ThreeColorsAdditional2;
        UpdatedAt = now;
        UpdatedByUserId = userId;
        return true;
    }

    public static string NormalizeCode(string modelCode)
    {
        var code = (modelCode ?? string.Empty).Trim().ToUpperInvariant();
        if (code.Length == 0 || code.Length > ModelCodeMaxLength)
            throw new ArgumentException("Model code is required.", nameof(modelCode));
        return code;
    }
}
