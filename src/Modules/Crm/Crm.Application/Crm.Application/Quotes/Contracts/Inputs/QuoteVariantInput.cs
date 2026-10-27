using Crm.Application.Quotes.Pricing;

namespace Crm.Application.Quotes.Contracts.Inputs;

[System.Text.Json.Serialization.JsonUnmappedMemberHandling(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)]

public sealed record QuoteVariantInput(
    Guid? PublicId,
    int SortOrder,
    string Title,
    bool Visible,
    QuotePriceIntent PriceIntent,
    Guid? BasisToken,
    QuoteTierVisibilityInput TierVisibility);
