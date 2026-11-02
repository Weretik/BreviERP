using Reference.Application.Features.PpeMarkupRule.Shared.DTOs;

namespace Reference.Application.Features.PpeMarkupRule.Create.DTOs;

public sealed record CreatePpeMarkupRuleCommandRequest(
    string Name,
    decimal RetailMarkup,
    decimal WholesaleMarkup,
    decimal LargeWholesaleMarkup,
    string? PriceNote = null,
    string? AppliesTo = null,
    int SortOrder = 0) : IPpeMarkupRuleFields;
