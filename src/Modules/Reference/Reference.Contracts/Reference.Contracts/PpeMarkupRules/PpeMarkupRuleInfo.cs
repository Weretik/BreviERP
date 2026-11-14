namespace Reference.Contracts.PpeMarkupRules;

/// <summary>A row of PPE markup rules as other modules see it.</summary>
public sealed record PpeMarkupRuleInfo(
    int Id, string Name, decimal RetailMarkup, decimal WholesaleMarkup, decimal LargeWholesaleMarkup);
