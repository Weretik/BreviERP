namespace Reference.Application.Features.PricingSettingsImport.DTOs;

/// <summary>
/// A row of the left table of «Налаштування цін». Percentages are fractions as in the sheet (0.25 = 25 %).
/// A calculated row («Розрахункове») carries no value.
/// </summary>
public sealed record PricingParameterImportRow(
    int RowNumber,
    string Code,
    string Group,
    string Name,
    decimal? Value,
    string Unit,
    bool IsCalculated,
    DateOnly? UpdatedOn,
    string? Comment);
