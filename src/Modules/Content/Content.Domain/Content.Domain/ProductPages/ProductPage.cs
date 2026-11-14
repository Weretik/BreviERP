using BuildingBlocks.Domain.Results;

namespace Content.Domain.ProductPages;

/// <summary>
/// The public site page of one Catalog product (CNT-002): whether the site shows it, its address and its texts in
/// Ukrainian and Russian. The product itself, its fabrics, prices and photos stay in other modules.
/// </summary>
public sealed class ProductPage
{
    private readonly List<ProductPageText> _texts = [];

    private ProductPage() { }

    private ProductPage(int productId, string slug, string updatedBy, DateTimeOffset now)
    {
        ProductId = productId;
        Slug = slug;
        Touch(updatedBy, now, bump: false);
    }

    /// <summary>A new page, hidden from the site until someone switches it on.</summary>
    public static DomainResult<ProductPage> Create(int productId, string slug, string updatedBy, DateTimeOffset now)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(productId);
        if (!ProductPageRules.IsValidSlug(slug)) return ProductPageErrors.InvalidSlug();
        return new ProductPage(productId, slug, updatedBy, now);
    }

    /// <summary>Catalog product id; no database link across modules.</summary>
    public int ProductId { get; private set; }

    public bool ShowOnSite { get; private set; }

    /// <summary>The address part after <c>/product/</c>, the same in both languages, unique among pages.</summary>
    public string Slug { get; private set; } = null!;

    /// <summary>Grows with every change; a save with an older number is refused.</summary>
    public int Revision { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    /// <summary>E-mail of the staff account that made the last change.</summary>
    public string UpdatedBy { get; private set; } = null!;

    public IReadOnlyList<ProductPageText> Texts => _texts;

    public ProductPageText? TextIn(string language) => _texts.FirstOrDefault(x => x.Language == language);

    public void SetVisibility(bool showOnSite, string updatedBy, DateTimeOffset now)
    {
        if (ShowOnSite == showOnSite) return;
        ShowOnSite = showOnSite;
        Touch(updatedBy, now, bump: true);
    }

    /// <summary>Replaces the address, the mark and all texts. A language without any text is not kept.</summary>
    public DomainResult Replace(string slug, bool showOnSite, IReadOnlyDictionary<string, ProductPageTextInput> texts,
        string updatedBy, DateTimeOffset now)
    {
        if (!ProductPageRules.IsValidSlug(slug)) return ProductPageErrors.InvalidSlug();
        Slug = slug;
        ShowOnSite = showOnSite;
        foreach (var language in ProductPageRules.Languages)
        {
            var text = TextIn(language);
            if (!texts.TryGetValue(language, out var input))
            {
                if (text is not null) _texts.Remove(text);
                continue;
            }

            if (text is null) _texts.Add(text = new ProductPageText(ProductId, language, input));
            else text.Apply(input);
            if (text.IsEmpty) _texts.Remove(text);
        }

        Touch(updatedBy, now, bump: true);
        return DomainResult.Success();
    }

    private void Touch(string updatedBy, DateTimeOffset now, bool bump)
    {
        UpdatedBy = updatedBy.Length > ProductPageRules.UpdatedByMaxLength
            ? updatedBy[..ProductPageRules.UpdatedByMaxLength]
            : updatedBy;
        UpdatedAtUtc = now.ToUniversalTime();
        if (bump) Revision++;
    }
}
