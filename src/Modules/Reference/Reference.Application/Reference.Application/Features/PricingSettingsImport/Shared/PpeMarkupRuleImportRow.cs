namespace Reference.Application.Features.PricingSettingsImport.Shared;

/// <summary>A row of the right table of «Налаштування цін»; markups are fractions (0.45 = 45 %).</summary>
public sealed record PpeMarkupRuleImportRow(
    int RowNumber,
    string Name,
    decimal RetailMarkup,
    decimal WholesaleMarkup,
    decimal LargeWholesaleMarkup,
    string? PriceNote,
    string? AppliesTo);
