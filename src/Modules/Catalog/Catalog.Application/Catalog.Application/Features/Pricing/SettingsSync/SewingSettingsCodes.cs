using Catalog.Domain.Pricing.Sewing;

namespace Catalog.Application.Features.Pricing.SettingsSync;

/// <summary>Sheet codes of «Налаштування цін» that make up <see cref="SewingCalculationSettings"/>.</summary>
public static class SewingSettingsCodes
{
    public static readonly IReadOnlyList<string> All =
    [
        "SALARY_SEAMSTRESS", "WORK_DAYS_MONTH", "MINUTES_SHIFT", "SEAMSTRESS_COUNT", "ASSEMBLY_RATE",
        "CUTTING_RATE", "MASTER_RATE", "SHOP_MANAGER_RATE", "SEAMSTRESS_BONUS", "MONTHLY_OVERHEAD",
        "SEWING_PRICE_COEF_1", "SEWING_PRICE_COEF_2", "SEWING_PRICE_COEF_3"
    ];

    /// <summary>Builds the settings, or returns the codes that are missing.</summary>
    public static (SewingCalculationSettings? Settings, IReadOnlyList<string> Missing) Map(
        IReadOnlyDictionary<string, decimal> values)
    {
        var missing = All.Where(code => !values.ContainsKey(code)).ToList();
        if (missing.Count != 0)
            return (null, missing);
        decimal V(string code) => values[code];
        return (new SewingCalculationSettings(V("SALARY_SEAMSTRESS"), V("WORK_DAYS_MONTH"), V("MINUTES_SHIFT"),
            V("SEAMSTRESS_COUNT"), V("ASSEMBLY_RATE"), V("CUTTING_RATE"), V("MASTER_RATE"),
            V("SHOP_MANAGER_RATE"), V("SEAMSTRESS_BONUS"), V("MONTHLY_OVERHEAD"),
            V("SEWING_PRICE_COEF_1"), V("SEWING_PRICE_COEF_2"), V("SEWING_PRICE_COEF_3")), []);
    }
}
