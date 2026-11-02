namespace Reference.Domain.PricingParameters;

/// <summary>
/// Formulas of the calculated rows of «Налаштування цін». A calculated parameter is never typed in by hand;
/// its value follows the parameters it is built from.
/// </summary>
public static class PricingParameterFormulas
{
    public const string MinuteCost = "MINUTE_COST_SEAMSTRESS";
    public const string MonthlySalary = "SALARY_SEAMSTRESS";
    public const string WorkDays = "WORK_DAYS_MONTH";
    public const string ShiftMinutes = "MINUTES_SHIFT";

    private static readonly Dictionary<string, Func<IReadOnlyDictionary<string, decimal>, decimal?>> Formulas =
        new(StringComparer.Ordinal)
        {
            // Вартість 1 хвилини роботи швачки = зарплата / (робочі дні × хвилини зміни), без премії.
            [MinuteCost] = values =>
                values.TryGetValue(MonthlySalary, out var salary)
                && values.TryGetValue(WorkDays, out var days)
                && values.TryGetValue(ShiftMinutes, out var minutes)
                && days * minutes > 0
                    ? salary / (days * minutes)
                    : null,
        };

    public static bool IsKnown(string code) => Formulas.ContainsKey(code);

    /// <summary>The value of a calculated parameter, or null when an input is missing.</summary>
    public static decimal? Calculate(string code, IReadOnlyDictionary<string, decimal> manualValues) =>
        Formulas.TryGetValue(code, out var formula) ? formula(manualValues) : null;
}
