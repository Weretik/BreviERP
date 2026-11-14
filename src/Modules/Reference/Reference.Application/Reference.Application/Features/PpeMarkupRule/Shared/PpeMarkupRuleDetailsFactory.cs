using Reference.Application.Features.PpeMarkupRule.Shared.DTOs;
using Reference.Domain.PpeMarkupRules;

namespace Reference.Application.Features.PpeMarkupRule.Shared;

internal static class PpeMarkupRuleDetailsFactory
{
    public static PpeMarkupRuleDetails From(IPpeMarkupRuleFields fields) => new(
        fields.RetailMarkup, fields.WholesaleMarkup, fields.LargeWholesaleMarkup, fields.PriceNote,
        fields.AppliesTo, fields.SortOrder);
}
