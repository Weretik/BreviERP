namespace Reference.Application.Features.PpeMarkupRule.Shared.DTOs;

/// <summary>Editable fields of a PPE markup rule; markups are fractions (0.45 = 45 %).</summary>
public interface IPpeMarkupRuleFields
{
    decimal RetailMarkup { get; }
    decimal WholesaleMarkup { get; }
    decimal LargeWholesaleMarkup { get; }
    string? PriceNote { get; }
    string? AppliesTo { get; }
    int SortOrder { get; }
}
