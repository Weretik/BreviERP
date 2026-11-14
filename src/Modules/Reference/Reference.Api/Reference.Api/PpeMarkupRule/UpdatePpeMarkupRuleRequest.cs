namespace Reference.Api.PpeMarkupRule;

/// <summary>Editable fields of a PPE markup rule; the name cannot change.</summary>
public sealed record UpdatePpeMarkupRuleRequest(
    decimal RetailMarkup,
    decimal WholesaleMarkup,
    decimal LargeWholesaleMarkup,
    string? PriceNote = null,
    string? AppliesTo = null,
    int SortOrder = 0);
