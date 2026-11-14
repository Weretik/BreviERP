using BuildingBlocks.Domain.Results;

namespace Production.Domain.FabricNorms;

/// <summary>
/// «Норми витрати тканини»: how much fabric one item of a Catalog sewing model needs. Owned by the workshop and keyed by
/// the model code (MOD-…); the model itself stays in Catalog. Orders of later stages plan and book fabric from it.
/// </summary>
public sealed class ModelFabricNorm
{
    public const int ModelCodeMaxLength = 32;

    private ModelFabricNorm() { }

    public static DomainResult<ModelFabricNorm> Create(string modelCode, FabricNormValues values, int? userId,
        DateTimeOffset now)
    {
        var code = CheckCode(modelCode);
        if (code.IsFailure) return code.Error!;
        var norm = new ModelFabricNorm { ModelCode = NormalizeCode(modelCode) };
        var set = norm.Set(values, userId, now);
        return set.IsFailure ? set.Error! : norm;
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
    public DomainResult<bool> Set(FabricNormValues values, int? userId, DateTimeOffset now)
    {
        var check = CheckValues(values);
        if (check.IsFailure) return check.Error!;
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

    /// <summary>Meters above zero with at most three decimals, and a main fabric for every additional one.</summary>
    public static DomainResult CheckValues(FabricNormValues values)
    {
        if (values.All().Any(x => !FabricNormValues.IsValidMeters(x)))
            return Error.Validation("production.norm_meters", "Норма — метри більше нуля, до трьох знаків після коми.", "values");
        if (!values.HasMainForEveryAdditional)
            return Error.Validation("production.norm_main_missing", "Для додаткової тканини вкажіть і основну.", "values");
        return DomainResult.Success();
    }

    /// <summary>The model code staff typed: required and at most <see cref="ModelCodeMaxLength"/> characters.</summary>
    public static DomainResult CheckCode(string? modelCode)
    {
        var code = (modelCode ?? string.Empty).Trim();
        return code.Length is > 0 and <= ModelCodeMaxLength
            ? DomainResult.Success()
            : Error.Validation("production.norm_model_code", "Вкажіть код моделі (до 32 символів).", "modelCode");
    }

    /// <summary>The upper-case code; the caller checked it with <see cref="CheckCode"/>.</summary>
    public static string NormalizeCode(string modelCode)
    {
        var code = (modelCode ?? string.Empty).Trim().ToUpperInvariant();
        if (code.Length == 0 || code.Length > ModelCodeMaxLength)
            throw new ArgumentException("Model code is required.", nameof(modelCode));
        return code;
    }
}
