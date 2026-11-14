using Reference.Application.Features.PpeMarkupRule.Shared;

namespace Reference.Application.Features.PpeMarkupRule.Commands.UpdatePpeMarkupRule;

public sealed record UpdatePpeMarkupRuleCommandRequest(
    decimal RetailMarkup,
    decimal WholesaleMarkup,
    decimal LargeWholesaleMarkup,
    string? PriceNote = null,
    string? AppliesTo = null,
    int SortOrder = 0) : IPpeMarkupRuleFields;
