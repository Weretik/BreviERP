namespace Reference.Api.PricingParameter;

/// <summary>Editable fields of a parameter; the code cannot change.</summary>
public sealed record UpdatePricingParameterRequest(
    string Group,
    string Name,
    decimal? Value,
    string Unit,
    bool IsCalculated = false,
    string? Comment = null,
    int SortOrder = 0);
