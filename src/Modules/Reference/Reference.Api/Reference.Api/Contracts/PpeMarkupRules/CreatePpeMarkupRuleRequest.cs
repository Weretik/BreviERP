namespace Reference.Api.Contracts.PpeMarkupRules;

/// <summary>A new PPE markup rule; markups are fractions (0.45 = 45 %).</summary>
public sealed record CreatePpeMarkupRuleRequest(
    string Name,
    decimal RetailMarkup,
    decimal WholesaleMarkup,
    decimal LargeWholesaleMarkup,
    string? PriceNote = null,
    string? AppliesTo = null,
    int SortOrder = 0);
