namespace Catalog.Domain.Pricing.Sewing;

public static class SewingSettingsValidation
{
    public static IReadOnlyList<SewingPricingBlocker> Validate(SewingCalculationSettings settings)
    {
        var errors = new List<SewingPricingBlocker>();
        if (settings.MonthlySalary <= 0 || settings.WorkDays <= 0 || settings.ShiftMinutes <= 0
            || settings.SeamstressCount <= 0 || settings.MonthlyOverhead < 0)
            errors.Add(new("InvalidSchedule", null, "Salary and work divisors must be positive; overhead must be nonnegative."));
        if (new[] { settings.AssemblyRate, settings.CuttingRate, settings.MasterRate, settings.ManagerRate, settings.BonusRate }
            .Any(rate => rate < 0 || rate > 1))
            errors.Add(new("InvalidRate", null, "Labor rates must be fractions between zero and one."));
        if (settings.RetailMultiplier <= 0 || settings.WholesaleMultiplier <= 0 || settings.LargeWholesaleMultiplier <= 0)
            errors.Add(new("InvalidMultiplier", null, "All tier multipliers must be positive."));
        var persistedValues = new[] { settings.MonthlySalary, settings.WorkDays, settings.ShiftMinutes,
            settings.SeamstressCount, settings.AssemblyRate, settings.CuttingRate, settings.MasterRate,
            settings.ManagerRate, settings.BonusRate, settings.MonthlyOverhead,
            settings.RetailMultiplier, settings.WholesaleMultiplier, settings.LargeWholesaleMultiplier };
        if (persistedValues.Any(value => decimal.Round(value, 8) != value
            || value > 999999999999.99999999m || value < -999999999999.99999999m))
            errors.Add(new("InvalidSettingPrecision", null, "Settings must fit numeric(20,8) without rounding."));
        return errors;
    }
}
