namespace Reference.Application.Features.PricingParameter.GetList.DTOs;

/// <summary>
/// A row of «Налаштування цін». <paramref name="Value"/> of a calculated parameter is computed from the other
/// parameters (null when an input is missing).
/// </summary>
public sealed record PricingParameterRowDTO(
    int Id,
    string Code,
    string Group,
    string Name,
    decimal? Value,
    string Unit,
    bool IsCalculated,
    DateOnly? UpdatedOn,
    string? Comment,
    int SortOrder);
