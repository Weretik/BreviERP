namespace Catalog.Domain.Pricing.Sewing;

public sealed class SewingPricingSettingsVersion
{
    public SewingPricingSettingsVersionId Id { get; private set; }
    public int Revision { get; private set; }
    public DateTimeOffset EffectiveFrom { get; private set; }
    public bool IsApproved { get; private set; }
    public string SourceHash { get; private set; } = null!;
    public string SourceJson { get; private set; } = null!;
    public string FormulaVersion { get; private set; } = null!;
    public decimal MonthlySalary { get; private set; }
    public decimal WorkDays { get; private set; }
    public decimal ShiftMinutes { get; private set; }
    public decimal SeamstressCount { get; private set; }
    public decimal AssemblyRate { get; private set; }
    public decimal CuttingRate { get; private set; }
    public decimal MasterRate { get; private set; }
    public decimal ManagerRate { get; private set; }
    public decimal BonusRate { get; private set; }
    public decimal MonthlyOverhead { get; private set; }
    public decimal RetailMultiplier { get; private set; }
    public decimal WholesaleMultiplier { get; private set; }
    public decimal LargeWholesaleMultiplier { get; private set; }

    private SewingPricingSettingsVersion() { }

    public static SewingPricingSettingsVersion Create(int revision, DateTimeOffset effectiveFrom,
        SewingCalculationSettings settings, string sourceHash, string sourceJson, bool isApproved)
    {
        if (revision <= 0 || sourceHash.Length != 64 || string.IsNullOrWhiteSpace(sourceJson))
            throw new ArgumentException("Revision and source provenance are required.");
        var validation = SewingSettingsValidation.Validate(settings);
        if (validation.Count != 0)
            throw new ArgumentException("Invalid sewing settings.");
        return new SewingPricingSettingsVersion
        {
            Revision = revision,
            EffectiveFrom = effectiveFrom.ToUniversalTime(),
            IsApproved = isApproved,
            SourceHash = sourceHash,
            SourceJson = sourceJson,
            FormulaVersion = SewingArticleCalculation.FormulaVersion,
            MonthlySalary = settings.MonthlySalary,
            WorkDays = settings.WorkDays,
            ShiftMinutes = settings.ShiftMinutes,
            SeamstressCount = settings.SeamstressCount,
            AssemblyRate = settings.AssemblyRate,
            CuttingRate = settings.CuttingRate,
            MasterRate = settings.MasterRate,
            ManagerRate = settings.ManagerRate,
            BonusRate = settings.BonusRate,
            MonthlyOverhead = settings.MonthlyOverhead,
            RetailMultiplier = settings.RetailMultiplier,
            WholesaleMultiplier = settings.WholesaleMultiplier,
            LargeWholesaleMultiplier = settings.LargeWholesaleMultiplier
        };
    }

    public SewingCalculationSettings ToSettings() => new(MonthlySalary, WorkDays, ShiftMinutes, SeamstressCount,
        AssemblyRate, CuttingRate, MasterRate, ManagerRate, BonusRate, MonthlyOverhead,
        RetailMultiplier, WholesaleMultiplier, LargeWholesaleMultiplier);
}

