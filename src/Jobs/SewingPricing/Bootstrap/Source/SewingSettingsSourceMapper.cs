using System.Text.Json;
using Catalog.Domain.Pricing.Sewing;

namespace SewingPricing.Jobs.Bootstrap;

public static class SewingSettingsSourceMapper
{
    public static SewingCalculationSettings Map(JsonElement[][] rows)
    {
        var values = rows.Skip(1).Where(row => row.Length > 3 && row[1].ValueKind == JsonValueKind.String)
            .ToDictionary(row => row[1].GetString()!, row => row[3].GetDecimal());
        decimal Value(string code) => values.TryGetValue(code, out var value)
            ? value : throw new InvalidDataException($"Missing pricing setting {code}.");
        return new(Value("SALARY_SEAMSTRESS"), Value("WORK_DAYS_MONTH"), Value("MINUTES_SHIFT"),
            Value("SEAMSTRESS_COUNT"), Value("ASSEMBLY_RATE"), Value("CUTTING_RATE"), Value("MASTER_RATE"),
            Value("SHOP_MANAGER_RATE"), Value("SEAMSTRESS_BONUS"), Value("MONTHLY_OVERHEAD"),
            Value("SEWING_PRICE_COEF_1"), Value("SEWING_PRICE_COEF_2"), Value("SEWING_PRICE_COEF_3"));
    }
}

