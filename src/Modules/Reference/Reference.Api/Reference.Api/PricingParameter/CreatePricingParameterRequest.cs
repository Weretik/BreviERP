namespace Reference.Api.PricingParameter;

/// <summary>A new parameter of «Налаштування цін»; percentages are fractions (0.25 = 25 %).</summary>
public sealed record CreatePricingParameterRequest(
    string Code,
    string Group,
    string Name,
    decimal? Value,
    string Unit,
    bool IsCalculated = false,
    string? Comment = null,
    int SortOrder = 0);
