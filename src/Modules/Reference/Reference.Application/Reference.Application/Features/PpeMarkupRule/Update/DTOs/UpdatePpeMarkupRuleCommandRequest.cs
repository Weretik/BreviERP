using Reference.Application.Features.PpeMarkupRule.Shared.DTOs;

namespace Reference.Application.Features.PpeMarkupRule.Update.DTOs;

public sealed record UpdatePpeMarkupRuleCommandRequest(
    decimal RetailMarkup,
    decimal WholesaleMarkup,
    decimal LargeWholesaleMarkup,
    string? PriceNote = null,
    string? AppliesTo = null,
    int SortOrder = 0) : IPpeMarkupRuleFields;
