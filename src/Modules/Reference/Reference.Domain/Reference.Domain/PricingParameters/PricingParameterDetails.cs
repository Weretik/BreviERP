namespace Reference.Domain.PricingParameters;

/// <summary>
/// One row of the Calculator sheet «Налаштування цін» (left table). <see cref="Value"/> is empty for a
/// calculated parameter: Brevi computes it from other parameters.
/// </summary>
public sealed record PricingParameterDetails(
    string Group,
    string Name,
    decimal? Value,
    string Unit,
    bool IsCalculated,
    string? Comment,
    int SortOrder);
