namespace Reference.Application.Features.PpeMarkupRule.GetList.DTOs;

/// <summary>A PPE markup rule; markups are fractions (0.45 = 45 %).</summary>
public sealed record PpeMarkupRuleRowDTO(
    int Id,
    string Name,
    decimal RetailMarkup,
    decimal WholesaleMarkup,
    decimal LargeWholesaleMarkup,
    string? PriceNote,
    string? AppliesTo,
    int SortOrder);
