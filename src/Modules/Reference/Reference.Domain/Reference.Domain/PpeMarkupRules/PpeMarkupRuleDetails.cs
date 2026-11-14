namespace Reference.Domain.PpeMarkupRules;

/// <summary>
/// One row of the right table of «Налаштування цін»: markups for a group of PPE products, as fractions
/// (0.45 = 45 %) for retail, wholesale and large wholesale.
/// </summary>
public sealed record PpeMarkupRuleDetails(
    decimal RetailMarkup,
    decimal WholesaleMarkup,
    decimal LargeWholesaleMarkup,
    string? PriceNote,
    string? AppliesTo,
    int SortOrder);
