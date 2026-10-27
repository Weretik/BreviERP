using Crm.Application.Quotes.Pricing;

namespace Crm.Application.Quotes.Contracts.Responses;

public sealed record QuoteVariantView(
    Guid PublicId, int SortOrder, string Title, string Kind, int? ProductId, long? ArticleId,
    bool Visible, QuotePricePreview PriceBasis, IReadOnlyList<QuoteTierView> Tiers);
