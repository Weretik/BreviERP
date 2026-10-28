using Crm.Domain.Quotes.Pricing;

namespace Crm.Domain.Quotes.Variants;

public sealed class QuoteVariant
{
    public long Id { get; private set; }
    public Guid PublicId { get; private set; }
    public long ItemId { get; private set; }
    public int SortOrder { get; private set; }
    public string Title { get; private set; } = null!;
    public string Kind { get; private set; } = null!;
    public int? ProductId { get; private set; }
    public long? ArticleId { get; private set; }
    public Crm.Domain.CustomSewing.Identifiers.CustomVersionId? CustomVersionId { get; private set; }
    public bool Visible { get; private set; }
    public Guid BasisToken { get; private set; }
    public string PriceBasisJson { get; private set; } = null!;
    private readonly List<QuoteTier> _tiers = [];
    public IReadOnlyCollection<QuoteTier> Tiers => _tiers;

    private QuoteVariant() { }

    public static QuoteVariant Create(
        Guid publicId, int sortOrder, string title, string kind, int? productId, long? articleId,
        bool visible, Guid basisToken, string priceBasisJson, IEnumerable<QuoteTier> tiers, Crm.Domain.CustomSewing.Identifiers.CustomVersionId? customVersionId = null)
    {
        if (sortOrder < 0 || string.IsNullOrWhiteSpace(title) || title.Length > 300 ||
            kind is not ("sewing" or "ppe" or "oneOffPpe" or "customSewing") ||
            basisToken == Guid.Empty || string.IsNullOrWhiteSpace(priceBasisJson) ||
            ((kind == "customSewing") != (customVersionId is { Value: > 0 })) ||
            (kind == "customSewing" && (productId is not null || articleId is not null)))
            throw new QuoteValidationException("invalid_variant");
        var variant = new QuoteVariant
        {
            PublicId = publicId == Guid.Empty ? Guid.NewGuid() : publicId,
            SortOrder = sortOrder,
            Title = title.Trim(),
            Kind = kind,
            ProductId = productId,
            ArticleId = articleId,
            CustomVersionId = customVersionId,
            Visible = visible,
            BasisToken = basisToken,
            PriceBasisJson = priceBasisJson
        };
        variant._tiers.AddRange(tiers);
        if (variant._tiers.Count != 3 || variant._tiers.Select(x => x.Code).Distinct().Count() != 3)
            throw new QuoteValidationException("invalid_tiers");
        return variant;
    }
}
